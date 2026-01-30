using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinCtrlICP
{
    public enum JoystickEventType
    {
        Added,
        Removed,
        ButtonDown,
        ButtonUp,
        AxisChange,
        PovUp,
        PovDown,
        PovLeft,
        PovRight,
        PovCenter
    }
}
