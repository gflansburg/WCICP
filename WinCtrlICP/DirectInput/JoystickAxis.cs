using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCtrlICP
{
    public enum Axis
    {
        X,
        Y,
        Z,
        RotationX,
        RotationY,
        RotationZ,
        Slider
    }

    public class JoystickAxis
    {
        public JoystickAxis()
        {
            Value = short.MaxValue;
        }

        public int Value { get; set; }
        public int Index { get; set; }
        public Axis Axis { get; set; }
        public DateTime ChangeTime { get; set; }
    }
}
