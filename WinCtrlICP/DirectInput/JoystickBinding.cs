using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCtrlICP
{
    public class JoystickBinding : IEquatable<JoystickBinding>
    {
        public BindingDeviceType DeviceType { get; set; }
        public int ButtonOrKey { get; set; }
        public bool LShift { get; set; }
        public bool LAlt { get; set; }
        public bool LControl { get; set; }
        public bool RShift { get; set; }
        public bool RAlt { get; set; }
        public bool RControl { get; set; }
        public Guid JoystickGuid { get; set; }
        public string Name { get; set; } = null!;
        public bool ReverseAxis { get; set; }
        public Direction Direction { get; set; }

        public bool Equals(JoystickBinding? other)
        {
            if (other == null || other.DeviceType != DeviceType || !(other.Name == Name) || !(other.JoystickGuid == JoystickGuid) || other.ReverseAxis != ReverseAxis || other.LAlt != LAlt || other.LControl != LControl || other.LShift != LShift || other.RAlt != RAlt || other.RControl != RControl || other.RShift != RShift || other.Direction != Direction)
            {
                return false;
            }
            return other.ButtonOrKey == ButtonOrKey;
        }

        public override string ToString()
        {
            switch (DeviceType)
            {
                case BindingDeviceType.None:
                    {
                        return "None";
                    }
                case BindingDeviceType.Keyboard:
                    {
                        List<string> keys = new List<string>();
                        if(LAlt)
                        {
                            keys.Add("LALT");
                        }
                        if(RAlt)
                        {
                            keys.Add("RALT");
                        }
                        if(LShift)
                        {
                            keys.Add("LSHIFT");
                        }
                        if(RShift)
                        {
                            keys.Add("RSHIFT");
                        }
                        if(LControl)
                        {
                            keys.Add("LCTRL");
                        }
                        if(RControl)
                        {
                            keys.Add("RCTRL");
                        }
                        keys.Add(((Keys)ButtonOrKey).ToString());
                        return string.Format("Keyboard: {0}", string.Join(" ", keys));
                    }
                case BindingDeviceType.JoystickButton:
                    {
                        return string.Format("Button {0} on {1}", ButtonOrKey + 1, Name);
                    }
                case BindingDeviceType.JoystickAxis:
                    {
                        return string.Format("Axis {0}{1} on {2}", Joystick.GetAxisType(ButtonOrKey), Joystick.GetAxisType(ButtonOrKey) == Axis.Slider ? " " + (ButtonOrKey - 6).ToString() : string.Empty, Name);
                    }
                case BindingDeviceType.JoystickPov:
                    {
                        return string.Format("POV {0} {1} on {2}", ButtonOrKey + 1, Direction.ToString(), Name);
                    }
                default:
                    {
                        return "None";
                    }
            }
        }
    }
}
