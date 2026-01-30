using FlightSim;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCtrlICP
{
    public class Joystick : IDisposable
    {
        public SharpDX.DirectInput.Joystick DXJoystick { get; private set; }
		public delegate void JoystickEventHandler(object sender, JoystickEventArgs e);
		public event JoystickEventHandler? JoystickEvent;
		private bool isPolling = true;
        private AbortableTaskRunner? _pollTask; 
		private List<JoystickButton> joystickButtons = new List<JoystickButton>();
		private List<JoystickAxis> joystickAxes = new List<JoystickAxis>();
		private List<JoystickPov> joystickPovs = new List<JoystickPov>();
		public bool IsDisposing { get; private set; }
		public bool IsDisposed { get; private set; }
		public Guid Guid { get; set; }

		public const double DEADZONE = .01;

        private volatile bool _needsResync = true;
        private DateTime _suppressButtonsUntilUtc = DateTime.MinValue;
        private const int STARTUP_SUPPRESS_MS = 250;
        // Add near other fields (top of class)
        private DateTime _suppressAxisLinkedButtonsUntilUtc = DateTime.MinValue;

        // How long after an axis change we ignore "phantom" button transitions.
        private const int AXIS_LINKED_BUTTON_SUPPRESS_MS = 60;

        // Buttons that are known to be emitted by axis movement (0-based indices).
        // User says "button 2", and your code uses 0-based buttonIndex.
        // If your UI shows 1-based numbering, you may need { 1 } instead of { 2 }.
        private static readonly HashSet<int> AXIS_LINKED_BUTTONS = new HashSet<int> { 2 };

        public Joystick(SharpDX.DirectInput.DirectInput directInput, Guid deviceGuid)
        {
            Guid = deviceGuid;
            DXJoystick = new SharpDX.DirectInput.Joystick(directInput, deviceGuid);
            DXJoystick.Properties.BufferSize = 128;  // or 256
            try
            {
				DXJoystick.Acquire();
				DXJoystick.Poll();
				LoadButtons();
				LoadAxes();
				LoadPovs();
			}
			catch (Exception)
            {
            }
            DXJoystick.GetBufferedData(); // flush initial garbage
            _needsResync = true;  // let PollLoop resync right before processing buffered events
            _pollTask = new AbortableTaskRunner();
            _suppressButtonsUntilUtc = DateTime.UtcNow.AddMilliseconds(STARTUP_SUPPRESS_MS);
            _pollTask.Start(PollLoop);
        }

		private void LoadButtons()
        {
			bool[] buttons = DXJoystick.GetCurrentState().Buttons;
			for (int i = 0; i < buttons.Length; i++)
			{
				joystickButtons.Add(new JoystickButton()
				{
					Pressed = buttons[i],
					Index = i,
					ChangeTime = DateTime.Now
				});
			}
		}

		private void LoadPovs()
		{
			int[] povs = DXJoystick.GetCurrentState().PointOfViewControllers;
			for (int i = 0; i < povs.Length; i++)
			{
				joystickPovs.Add(new JoystickPov()
				{
					Direction = (Direction)povs[i],
					Index = i,
					ChangeTime = DateTime.Now
				});
			}
		}

		public static Axis GetAxisType(int index)
        {
			switch(index)
            {
				case 0:
					return Axis.X;
				case 1:
					return Axis.Y;
				case 2:
					return Axis.Z;
				case 3:
					return Axis.RotationX;
				case 4:
					return Axis.RotationY;
				case 5:
					return Axis.RotationZ;
				default:
					return Axis.Slider;
            }
        }

		private int GetAxisValue(int index, SharpDX.DirectInput.JoystickState state)
        {
			switch(index)
            {
				case 0:
					return state.X;
				case 1:
					return state.Y;
				case 2:
					return state.Z;
				case 3:
					return state.RotationX;
				case 4:
					return state.RotationY;
				case 5:
					return state.RotationZ;
				default:
					if (index - 6 < state.Sliders.Length)
					{
						return state.Sliders[index - 6];
					}
					return short.MaxValue;
            }
        }

		private void LoadAxes()
		{
			SharpDX.DirectInput.JoystickState state = DXJoystick.GetCurrentState();
			int axeCount = 6 + state.Sliders.Length;
			for(int i = 0; i < axeCount; i++)
			{
				joystickAxes.Add(new JoystickAxis()
				{
					Value = GetAxisValue(i, state),
					Axis = GetAxisType(i),
					ChangeTime = DateTime.Now
				});
			}
		}

		private JoystickEventType GetPovEventType(Direction direction)
        {
			switch (direction)
            {
				case Direction.Down:
					return JoystickEventType.PovDown;
				case Direction.Left:
					return JoystickEventType.PovLeft;
				case Direction.Right:
					return JoystickEventType.PovRight;
				case Direction.Up:
					return JoystickEventType.PovUp;
            }
			return JoystickEventType.PovCenter;
        }

        private async Task PollLoop(CancellationToken token)
        {
            // Reduce CPU churn while remaining responsive.
            const int pollDelayMs = 5;

            while (!token.IsCancellationRequested && isPolling && !IsDisposing && !IsDisposed)
            {
                try
                {
                    if (DXJoystick == null || DXJoystick.IsDisposed)
                        break;

                    if (_needsResync)
                    {
                        ResyncStateNoEvents();
                        _needsResync = false;
                    }

                    // Poll can throw InputLost/NotAcquired depending on focus/device state.
                    DXJoystick.Poll();

                    // Buffered events prevents missing short taps during brief stalls.
                    var updates = DXJoystick.GetBufferedData();
                    if (updates != null && updates.Length > 0)
                    {
                        foreach (var u in updates)
                        {
                            if (token.IsCancellationRequested || IsDisposing || IsDisposed)
                                break;

                            // --------------------------
                            // BUTTONS (Offsets 0..127)
                            // --------------------------
                            // SharpDX uses JoystickOffset.Buttons0 .. Buttons127
                            // We'll map Buttons0 => index 0, Buttons1 => index 1, etc.
                            if (u.Offset >= SharpDX.DirectInput.JoystickOffset.Buttons0 &&
                                u.Offset <= SharpDX.DirectInput.JoystickOffset.Buttons127)
                            {
                                int buttonIndex = (int)u.Offset - (int)SharpDX.DirectInput.JoystickOffset.Buttons0;
                                bool pressed = u.Value != 0;

                                // Guard for devices with fewer buttons than the max enum range
                                if (buttonIndex >= 0 && buttonIndex < joystickButtons.Count)
                                {
                                    if (joystickButtons[buttonIndex].Pressed != pressed)
                                    {
                                        joystickButtons[buttonIndex].Pressed = pressed;
                                        joystickButtons[buttonIndex].ChangeTime = DateTime.Now;

                                        // Suppress startup/resync phantom button transitions
                                        if (DateTime.UtcNow < _suppressButtonsUntilUtc)
                                            continue;

                                        JoystickEvent?.Invoke(this, new JoystickEventArgs
                                        {
                                            Joystick = DXJoystick,
                                            ButtonOrAxis = buttonIndex,
                                            EventType = pressed ? JoystickEventType.ButtonDown : JoystickEventType.ButtonUp,
                                            Value = pressed ? 1 : 0
                                        });
                                    }
                                }

                                continue;
                            }

                            // --------------------------
                            // POV hats
                            // --------------------------
                            // POV offsets can show as PointOfViewControllers0..3 in SharpDX.
                            if (u.Offset >= SharpDX.DirectInput.JoystickOffset.PointOfViewControllers0 &&
                                u.Offset <= SharpDX.DirectInput.JoystickOffset.PointOfViewControllers3)
                            {
                                int povIndex = (int)u.Offset - (int)SharpDX.DirectInput.JoystickOffset.PointOfViewControllers0;

                                if (povIndex >= 0 && povIndex < joystickPovs.Count)
                                {
                                    // u.Value is typically:
                                    //  -1 (center) or 0/9000/18000/27000, etc.
                                    var newDir = (Direction)u.Value;

                                    if ((int)joystickPovs[povIndex].Direction != (int)newDir)
                                    {
                                        joystickPovs[povIndex].Direction = newDir;
                                        joystickPovs[povIndex].ChangeTime = DateTime.Now;

                                        JoystickEvent?.Invoke(this, new JoystickEventArgs
                                        {
                                            Joystick = DXJoystick,
                                            ButtonOrAxis = povIndex,
                                            Direction = newDir,
                                            EventType = GetPovEventType(newDir),
                                            Value = u.Value
                                        });
                                    }
                                }

                                continue;
                            }

                            // --------------------------
                            // AXES
                            // --------------------------
                            // For axes, the buffered offset names are X/Y/Z/RotationX/RotationY/RotationZ/Sliders0.1, etc.
                            // We map to your index convention:
                            //   0=X,1=Y,2=Z,3=RotX,4=RotY,5=RotZ,6+=Sliders
                            int? axisIndex = null;
                            int axisValue = u.Value;

                            switch (u.Offset)
                            {
                                case SharpDX.DirectInput.JoystickOffset.X: axisIndex = 0; break;
                                case SharpDX.DirectInput.JoystickOffset.Y: axisIndex = 1; break;
                                case SharpDX.DirectInput.JoystickOffset.Z: axisIndex = 2; break;
                                case SharpDX.DirectInput.JoystickOffset.RotationX: axisIndex = 3; break;
                                case SharpDX.DirectInput.JoystickOffset.RotationY: axisIndex = 4; break;
                                case SharpDX.DirectInput.JoystickOffset.RotationZ: axisIndex = 5; break;
                                case SharpDX.DirectInput.JoystickOffset.Sliders0: axisIndex = 6; break;
                                case SharpDX.DirectInput.JoystickOffset.Sliders1: axisIndex = 7; break;
                            }

                            if (axisIndex.HasValue)
                            {
                                int i = axisIndex.Value;

                                if (i >= 0 && i < joystickAxes.Count)
                                {
                                    int minValue = axisValue - (int)(ushort.MaxValue * DEADZONE);
                                    int maxValue = axisValue + (int)(ushort.MaxValue * DEADZONE);

                                    if (joystickAxes[i].Value < minValue || joystickAxes[i].Value > maxValue)
                                    {
                                        joystickAxes[i].Value = axisValue;
                                        joystickAxes[i].ChangeTime = DateTime.Now;

                                        JoystickEvent?.Invoke(this, new JoystickEventArgs
                                        {
                                            Joystick = DXJoystick,
                                            ButtonOrAxis = i,
                                            EventType = JoystickEventType.AxisChange,
                                            Value = axisValue
                                        });
                                    }
                                }

                                continue;
                            }

                            // Unknown/unsupported offset => ignore
                        }
                    }
                    else
                    {
                        // No buffered data (or buffer empty). Optional fallback:
                        // You *can* keep the old GetCurrentState diff here if you want,
                        // but buffered input already covers missed taps.
                    }
                }
                catch (SharpDX.SharpDXException ex) when (
                    ex.ResultCode == SharpDX.DirectInput.ResultCode.InputLost ||
                    ex.ResultCode == SharpDX.DirectInput.ResultCode.NotAcquired)
                {
                    _needsResync = true;
                }
                catch
                {
                    // Don't hard-fail the thread. Consider logging if you can.
                }

                try
                {
                    await Task.Delay(pollDelayMs, token);
                }
                catch
                {
                    // ignore cancellation
                }
            }
        }


        private void ResyncStateNoEvents()
        {
            try
            {
                DXJoystick.Acquire();
                DXJoystick.Poll();

                // Update baselines from current state
                var state = DXJoystick.GetCurrentState();

                // Buttons
                var buttons = state.Buttons;
                if (joystickButtons.Count == 0)
                {
                    joystickButtons.Clear();
                    for (int i = 0; i < buttons.Length; i++)
                        joystickButtons.Add(new JoystickButton { Pressed = buttons[i], Index = i, ChangeTime = DateTime.UtcNow });
                }
                else
                {
                    int n = Math.Min(buttons.Length, joystickButtons.Count);
                    for (int i = 0; i < n; i++)
                        joystickButtons[i].Pressed = buttons[i];
                }

                // POVs
                var povs = state.PointOfViewControllers;
                if (joystickPovs.Count == 0)
                {
                    joystickPovs.Clear();
                    for (int i = 0; i < povs.Length; i++)
                        joystickPovs.Add(new JoystickPov { Direction = (Direction)povs[i], Index = i, ChangeTime = DateTime.UtcNow });
                }
                else
                {
                    int n = Math.Min(povs.Length, joystickPovs.Count);
                    for (int i = 0; i < n; i++)
                        joystickPovs[i].Direction = (Direction)povs[i];
                }

                // Axes
                int axisCount = 6 + state.Sliders.Length;
                if (joystickAxes.Count == 0)
                {
                    joystickAxes.Clear();
                    for (int i = 0; i < axisCount; i++)
                        joystickAxes.Add(new JoystickAxis { Value = GetAxisValue(i, state), Axis = GetAxisType(i), ChangeTime = DateTime.UtcNow });
                }
                else
                {
                    int n = Math.Min(axisCount, joystickAxes.Count);
                    for (int i = 0; i < n; i++)
                        joystickAxes[i].Value = GetAxisValue(i, state);
                }

                // Flush buffered events so we don't process stale transitions after (re)acquire
                DXJoystick.GetBufferedData();
            }
            catch
            {
                // ignore; we'll try again later
            }
        }

        public void Dispose()
        {
            if (IsDisposed || IsDisposing)
                return;
            IsDisposing = true;
            isPolling = false;
            try
            {
                StopPolling(1000).GetAwaiter().GetResult();
            }
            catch 
            {
            }
            try
            {
                if (DXJoystick != null && !DXJoystick.IsDisposed)
                {
                    JoystickEvent?.Invoke(this, new JoystickEventArgs
                    {
                        Joystick = DXJoystick,
                        ButtonOrAxis = -1,
                        EventType = JoystickEventType.Removed,
                        Value = 0
                    });

                    DXJoystick.Dispose();
                }
            }
            catch { }
            IsDisposed = true;
            IsDisposing = false;
        }

        public async Task StopPolling(int timeOut = 1000)
        {
            isPolling = false;
            if (_pollTask != null && _pollTask.IsRunning)
            {
                await _pollTask.StopAsync(timeOut).ConfigureAwait(false);
            }
            _pollTask?.Dispose();
            _pollTask = null;
        }
    }
}
