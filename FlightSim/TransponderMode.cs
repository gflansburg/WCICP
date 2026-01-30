using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSim
{
    public enum TransponderMode
    {
        Off = 0,
        Standby = 1,
        Test = 2,
        On_Mode_A = 3,
        Alt_Mode_C = 4,
        Ground_Mode_S = 5
    }

    public enum XPlaneTransponderMode
    {
        Off = 0, 
        Standby = 1, 
        On_Mode_A = 2, 
        Alt_Mode_C = 3, 
        Test = 4, 
        Ground_Mode_S = 5, 
        TA_Only_Mode_S = 6,
        TA_RA_Mode_S = 7
    }
}
