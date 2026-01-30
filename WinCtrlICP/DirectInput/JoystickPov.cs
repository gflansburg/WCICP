using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCtrlICP
{
    public enum Direction
    {
        None = -1,
        Up = 0,
        Right = 9000,
        Down = 18000,
        Left = 27000
    }
    public class JoystickPov
    {
        public JoystickPov()
        {
            Direction = Direction.None;
        }

        public Direction Direction { get; set; }
        public int Index { get; set; }
        public DateTime ChangeTime { get; set; }
    }
}
