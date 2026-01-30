using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCtrlICP
{
    public class JoystickButton
    {
        public bool Pressed { get; set; }
        public int Index { get; set; }
        public DateTime ChangeTime { get; set; }
    }
}
