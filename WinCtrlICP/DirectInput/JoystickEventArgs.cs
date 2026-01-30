using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCtrlICP
{
	public class JoystickEventArgs : EventArgs
	{
		public JoystickEventArgs()
        {
			Direction = Direction.None;
        }
		public JoystickEventType EventType { get; set; }
		public int ButtonOrAxis { get; set; }
		public int Value { get; set; } 
		public Direction Direction { get; set; }
		public SharpDX.DirectInput.Joystick Joystick { get; set; } = null!;
	}
}
