using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlightSim
{
    public sealed class AbortableTaskRunner : IDisposable
    {
        private CancellationTokenSource? _cts;
        private Task? _task;

        public bool Stop { get; private set; }
        public bool IsRunning => _task is { IsCompleted: false };

        public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

        public void Start(Func<CancellationToken, Task> work)
        {
            if (IsRunning) throw new InvalidOperationException("Task is already running.");

            Stop = false;
            _cts = new CancellationTokenSource();

            // LongRunning is optional; only use if you truly need a dedicated thread.
            _task = Task.Factory.StartNew(
                () => work(_cts.Token),
                _cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            ).Unwrap();
        }

        public async Task StopAsync(int timeoutMs = 100)
        {
            Stop = true;

            if (_cts is null || _task is null)
                return;

            try
            {
                _cts.Cancel();
            }
            catch { /* ignore */ }

            // Give it a short window to exit cooperatively.
            var finished = await Task.WhenAny(_task, Task.Delay(timeoutMs)).ConfigureAwait(false);

            // We do NOT "kill" threads anymore.
            // If it didn't finish, it will continue running until the work cooperates and exits.
            // That's the key contract you need to enforce in the work loop.
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
