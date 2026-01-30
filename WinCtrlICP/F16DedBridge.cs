using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace WinCtrlICP
{
    public class F16DedBridge : IDisposable
    {
        private const string PipeName = "WCICP_F16DED";

        private readonly SemaphoreSlim _ioGate = new(1, 1);
        private volatile bool _stopping;

        // Bump this whenever you rebuild the Electron bundle
        private const string BridgeVersion = "1.0.0";

        // The exe inside your zip root
        private const string BridgeExeName = "F16DEDWriter.exe";

        // The embedded resource name - adjust if needed
        // Tip: inspect Assembly.GetExecutingAssembly().GetManifestResourceNames()
        private const string ZipResourceName = "WinCtrlICP.F16DEDWriter.zip";

        private NamedPipeClientStream? _pipe;
        private StreamReader? _reader;
        private StreamWriter? _writer;

        private Process? _launchedProcess; // only set if WE launched it

        // --- Restart supervision (service-style recovery) ---
        private readonly object _restartLock = new();

        private int _restartFailures = 0;
        private DateTime _lastSuccessfulStartUtc = DateTime.MinValue;

        private CancellationTokenSource? _superviseCts;
        private Task? _superviseTask;

        private static readonly TimeSpan RestartResetWindow = TimeSpan.FromHours(1);
        private const int MaxRestartFailures = 3;

        // Optional: let the app know we gave up
        public event Action<string>? OnBridgeFatal;

        private long _nextId;

        private string? _bridgeDir;

        public F16DedBridge()
        {
            EnsureExtracted(useRoaming: false);
        }

        public string EnsureExtracted(bool useRoaming = false)
        {
            _bridgeDir = Path.Combine(
                Environment.GetFolderPath(useRoaming
                    ? Environment.SpecialFolder.ApplicationData      // Roaming
                    : Environment.SpecialFolder.LocalApplicationData // Local
                ),
                "WCICP",
                "F16DEDWriter",
                BridgeVersion
            );

            var exePath = Path.Combine(_bridgeDir, BridgeExeName);
            if (File.Exists(exePath))
                return _bridgeDir;

            Directory.CreateDirectory(_bridgeDir);

            // Extract to temp folder first
            var tmpDir = _bridgeDir + ".tmp";
            if (Directory.Exists(tmpDir))
                Directory.Delete(tmpDir, recursive: true);

            Directory.CreateDirectory(tmpDir);

            ExtractEmbeddedZipToDirectory(ZipResourceName, tmpDir);

            // Validate extraction
            var tmpExe = Path.Combine(tmpDir, BridgeExeName);
            if (!File.Exists(tmpExe))
                throw new InvalidOperationException($"Extraction succeeded but {BridgeExeName} was not found in the zip root.");

            // If _bridgeDir exists but exe missing, wipe it and replace
            if (Directory.Exists(_bridgeDir))
            {
                try { Directory.Delete(_bridgeDir, recursive: true); }
                catch { /* if locked, let it fail loud later */ }
            }

            Directory.Move(tmpDir, _bridgeDir);

            // Final check
            if (!File.Exists(exePath))
                throw new InvalidOperationException("Bridge install failed: exe still missing after move.");

            return _bridgeDir;
        }

        private static void ExtractEmbeddedZipToDirectory(string resourceName, string destinationDir)
        {
            var asm = Assembly.GetExecutingAssembly();
            using Stream? s = asm.GetManifestResourceStream(resourceName);
            if (s == null)
                throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

            using var zip = new ZipArchive(s, ZipArchiveMode.Read);
            foreach (var entry in zip.Entries)
            {
                // Directories have empty Name
                var fullPath = Path.Combine(destinationDir, entry.FullName);

                // Security: prevent zip-slip
                var fullPathNormalized = Path.GetFullPath(fullPath);
                var destNormalized = Path.GetFullPath(destinationDir) + Path.DirectorySeparatorChar;
                if (!fullPathNormalized.StartsWith(destNormalized, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Zip contained an invalid path (zip-slip).");

                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(fullPath);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                entry.ExtractToFile(fullPath, overwrite: true);
            }
        }

        public async Task ConnectOrLaunchAsync(int connectTimeoutMs = 1200, int launchWaitMs = 8000)
        {
            if (await TryConnectAsync(connectTimeoutMs).ConfigureAwait(false))
            {
                StartSupervisionIfNeeded();
                return;
            }

            _launchedProcess = StartHeadless();

            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < launchWaitMs)
            {
                if (await TryConnectAsync(connectTimeoutMs).ConfigureAwait(false))
                {
                    StartSupervisionIfNeeded();
                    return;
                }

                await Task.Delay(200).ConfigureAwait(false);
            }

            throw new TimeoutException("Launched F16DEDWriter.exe but could not connect to named pipe '" + PipeName + "'.");
        }

        public Process StartHeadless()
        {
            var exePath = Path.Combine(_bridgeDir ?? string.Empty, BridgeExeName);

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
#if DEBUG
                Arguments = "--headless --logging",
#else
                Arguments = "--headless",
#endif
                WorkingDirectory = _bridgeDir ?? string.Empty, // CRITICAL so resources/app.asar paths behave
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
            p.Start();
            return p;
        }

        private void StartSupervisionIfNeeded()
        {
            lock (_restartLock)
            {
                if (_superviseTask != null && !_superviseTask.IsCompleted)
                    return;

                _superviseCts?.Dispose();
                _superviseCts = new CancellationTokenSource();
                var token = _superviseCts.Token;

                // Record a "successful start" moment (used for failure window reset)
                _lastSuccessfulStartUtc = DateTime.UtcNow;

                _superviseTask = Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested && !_stopping)
                    {
                        Process? p = _launchedProcess;

                        // If we didn't launch it, we can't reliably supervise process exit.
                        // Still: if the pipe drops, the next draw will throw and your outer logic can reconnect.
                        if (p == null)
                        {
                            await Task.Delay(1000, token).ConfigureAwait(false);
                            continue;
                        }

                        try
                        {
                            // Wait until it exits (poll so we can cancel)
                            while (!token.IsCancellationRequested && !_stopping && !p.HasExited)
                                await Task.Delay(500, token).ConfigureAwait(false);

                            if (token.IsCancellationRequested || _stopping)
                                break;

                            // Unexpected exit -> attempt restart
                            await HandleUnexpectedExitAndMaybeRestartAsync(token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            // If supervision itself errors, don't spin.
                            OnBridgeFatal?.Invoke("Bridge supervision error: " + ex.Message);
                            break;
                        }
                    }
                }, token);
            }
        }

        private async Task HandleUnexpectedExitAndMaybeRestartAsync(CancellationToken token)
        {
            // Close any dead pipe handles immediately
            SafeClosePipe();

            var now = DateTime.UtcNow;

            // Reset the "failure burst" if the last run was stable long enough
            if (_lastSuccessfulStartUtc != DateTime.MinValue &&
                (now - _lastSuccessfulStartUtc) >= RestartResetWindow)
            {
                _restartFailures = 0;
            }

            _restartFailures++;

            if (_restartFailures >= MaxRestartFailures)
            {
                OnBridgeFatal?.Invoke($"Bridge crashed {_restartFailures} times in a row. Giving up.");
                return;
            }

            // Small backoff to avoid rapid spin
            await Task.Delay(1000, token).ConfigureAwait(false);

            // Try to relaunch + reconnect
            try
            {
                // Dispose old process object if needed
                try { _launchedProcess?.Dispose(); } catch { }
                _launchedProcess = null;

                await ConnectOrLaunchAsync().ConfigureAwait(false);

                // If we got here, it stayed up at least long enough to connect;
                // mark start time for stability window tracking.
                _lastSuccessfulStartUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // Count this as another failure “immediately”
                OnBridgeFatal?.Invoke("Bridge restart attempt failed: " + ex.Message);

                // Let loop continue; next exit/retry happens when/if we try again,
                // but we can also trigger another immediate attempt here if you want.
            }
        }

        private async Task<bool> TryConnectAsync(int timeoutMs)
        {
            try
            {
                var pipe = new NamedPipeClientStream(
                    serverName: ".",
                    pipeName: PipeName,
                    direction: PipeDirection.InOut,
                    options: PipeOptions.Asynchronous);

                using var cts = new CancellationTokenSource(timeoutMs);

                // Critical for "sync wait" from WinForms UI thread
                await pipe.ConnectAsync(cts.Token).ConfigureAwait(false);

                // Wire up reader/writer
                _pipe = pipe;
                _reader = new StreamReader(
                    _pipe,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 4096,
                    leaveOpen: true);

                _writer = new StreamWriter(
                    _pipe,
                    new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                    bufferSize: 4096,
                    leaveOpen: true)
                { AutoFlush = true };

                // If PingAsync uses awaits, it also MUST be ConfigureAwait(false) inside
                await PingAsync().ConfigureAwait(false);

                return true;
            }
            catch (OperationCanceledException)
            {
                // timeout/cancel
                SafeClosePipe();
                return false;
            }
            catch (Exception)
            {
                // connect failed
                SafeClosePipe();
                return false;
            }
        }

        public Task DrawIcpAsync(string[] lines, string font = "DCS")
        {
            // During shutdown: ignore periodic draw spam
            if (_stopping) return Task.CompletedTask;
            return CallAsync("drawIcp", new { lines, font });
        }

        public Task ClearIcpAsync()
        {
            // Allowed during shutdown (ShutdownAsync uses it)
            return CallAsync("clearIcp", new { });
        }

        public Task PingAsync() => CallAsync("ping", new { });


        /// <summary>
        /// Requirement (3): blank lines then terminate F16DEDWriter.exe.
        /// </summary>
        public async Task ShutdownAsync(int timeout = 1500)
        {
            _stopping = true;

            try { _superviseCts?.Cancel(); } catch { }

            // Wait for any in-flight CallAsync (e.g., drawIcp) to finish, then own the stream
            await _ioGate.WaitAsync().ConfigureAwait(false);
            try
            {
                try { await CallAsyncCore("clearIcp", new { }, timeoutMs: timeout).ConfigureAwait(false); } catch { }

                await Task.Delay(150).ConfigureAwait(false);

                try { await CallAsyncCore("close", new { }, timeoutMs: timeout).ConfigureAwait(false); } catch { }
            }
            finally
            {
                _ioGate.Release();
            }

            // If we launched it, ensure it's dead
            if (_launchedProcess != null)
            {
                try
                {
                    if (!_launchedProcess.HasExited)
                    {
                        if (!_launchedProcess.WaitForExit(timeout))
                            _launchedProcess.Kill(entireProcessTree: true);
                    }
                }
                catch { /* ignore */ }
            }

            SafeClosePipe();
        }

        private async Task CallAsync(string cmd, object arg, int timeoutMs = 5000)
        {
            // Once stopping begins, block everything except shutdown-critical ops
            if (_stopping && cmd is not ("clearIcp" or "close" or "ping"))
                throw new OperationCanceledException("Bridge is shutting down.");

            await _ioGate.WaitAsync().ConfigureAwait(false);
            try
            {
                await CallAsyncCore(cmd, arg, timeoutMs).ConfigureAwait(false);
            }
            finally
            {
                _ioGate.Release();
            }
        }

        private async Task CallAsyncCore(string cmd, object arg, int timeoutMs)
        {
            if (_pipe == null || _reader == null || _writer == null || !_pipe.IsConnected)
                throw new IOException("Not connected to named pipe.");

            var id = Interlocked.Increment(ref _nextId);

            var payload = JsonSerializer.Serialize(new { id, cmd, arg });
            await _writer.WriteLineAsync(payload).ConfigureAwait(false);

            using var cts = new CancellationTokenSource(timeoutMs);

            while (!cts.IsCancellationRequested)
            {
                var lineTask = _reader.ReadLineAsync();
                var completed = await Task.WhenAny(lineTask, Task.Delay(Timeout.Infinite, cts.Token))
                    .ConfigureAwait(false);

                if (completed != lineTask)
                    break;

                var line = await lineTask.ConfigureAwait(false);
                if (line == null)
                    throw new IOException("Pipe closed by server.");

                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;

                if (!root.TryGetProperty("id", out var idEl) || idEl.ValueKind != JsonValueKind.Number)
                    continue;

                var respId = idEl.GetInt64();
                if (respId != id)
                    continue;

                var ok = root.TryGetProperty("ok", out var okEl) && okEl.ValueKind == JsonValueKind.True;
                if (ok)
                    return;

                var err = root.TryGetProperty("error", out var errEl) ? errEl.GetString() : "Unknown error";
                throw new Exception(err);
            }

            throw new TimeoutException($"Timeout waiting for '{cmd}' (id={id}).");
        }

        private void SafeClosePipe()
        {
            try { _writer?.Dispose(); } catch { }
            try { _reader?.Dispose(); } catch { }
            try { _pipe?.Dispose(); } catch { }

            _writer = null;
            _reader = null;
            _pipe = null;
        }

        public void Dispose()
        {
            try { _superviseCts?.Cancel(); } catch { }
            try { _superviseCts?.Dispose(); } catch { }
            SafeClosePipe();
            try { _launchedProcess?.Dispose(); } catch { }
        }
    }
}