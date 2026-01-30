using FlightSim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCtrlICP
{
    public class Joysticks : IDisposable
    {
        private readonly SharpDX.DirectInput.DirectInput directInput = new SharpDX.DirectInput.DirectInput();
        private readonly Dictionary<Guid, Joystick> joysticks = new Dictionary<Guid, Joystick>();
		public delegate void JoystickEventHandler(object sender, JoystickEventArgs e);
		public event JoystickEventHandler? JoystickEvent;
		private bool isPolling = true;
        private AbortableTaskRunner? _pollTask;
        public bool IsDisposing { get; private set; }
		public bool IsDisposed { get; private set; }
        private readonly object _sync = new object();

        public Joysticks()
        {
            LoadJoysticks();
			_pollTask = new AbortableTaskRunner();
			_pollTask.Start(PollLoop);
        }

        private async Task PollLoop(CancellationToken token)
        {
            while(!token.IsCancellationRequested && isPolling && !IsDisposing && !IsDisposed)
            {
				LoadJoysticks();
                await Task.Delay(5000, token);
            }
		}

        public void Dispose()
        {
			if (!IsDisposed && !IsDisposing)
			{
				IsDisposing = true;
				isPolling = false;
                try
                {
                    StopPolling(1000).GetAwaiter().GetResult();
                }
                catch 
				{ 
				}
                CloseJoysticks();
				IsDisposed = true;
				IsDisposing = false;
			}
        }

		private void LoadJoysticks()
		{
			try
			{
				IList<SharpDX.DirectInput.DeviceInstance> devices = directInput.GetDevices(SharpDX.DirectInput.DeviceClass.GameControl, SharpDX.DirectInput.DeviceEnumerationFlags.AttachedOnly);
				foreach (SharpDX.DirectInput.DeviceInstance device in devices)
				{
					if (IsDisposing || IsDisposed)
					{
						break;
					}
					if (!directInput.IsDeviceAttached(device.InstanceGuid))
					{
						continue;
					}
					lock (_sync)
					{
						if (!joysticks.ContainsKey(device.InstanceGuid))
						{
							Joystick joystick = new Joystick(directInput, device.InstanceGuid);
							joysticks.Add(device.InstanceGuid, joystick);
							JoystickEvent?.Invoke(this, new JoystickEventArgs()
							{
								Joystick = joystick.DXJoystick,
								ButtonOrAxis = -1,
								EventType = JoystickEventType.Added,
								Value = 0
							});
							joystick.JoystickEvent += Joystick_JoystickEvent;
						}
					}
				}
				if (!IsDisposed && !IsDisposing)
				{
					lock (_sync)
					{
						List<Joystick> remove = new List<Joystick>();
						foreach (Guid guid in joysticks.Keys)
						{
							Joystick joystick = joysticks[guid];
							if (IsDisposing || IsDisposed)
							{
								break;
							}
							if (devices.FirstOrDefault(j => j.InstanceGuid == joystick.Guid) == null)
							{
								remove.Add(joystick);
							}
						}
						if (!IsDisposed && !IsDisposing)
						{
							foreach (Joystick joystick in remove)
							{
								if (IsDisposing || IsDisposed)
								{
									break;
								}
								joystick.Dispose();
								lock (_sync)
								{
									joysticks.Remove(joystick.Guid);
								}
							}
						}
					}
				}
			}
			catch(Exception)
            {
            }
		}

        private void Joystick_JoystickEvent(object sender, JoystickEventArgs e)
        {
			JoystickEvent?.Invoke(sender, e);
        }

        private void CloseJoysticks()
		{
			lock (_sync)
			{
				if (joysticks != null)
				{
					foreach (Guid guid in joysticks.Keys)
					{
						joysticks[guid].Dispose();
					}
					joysticks.Clear();
				}
			}
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
