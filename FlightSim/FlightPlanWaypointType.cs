using System;
using System.Collections.Generic;
using System.Text;

namespace FlightSim
{
    public enum FlightPlanWaypointType
    {
        NONE = 0,
        AIRPORT = 1,
        INTRSEC = 2,
        VOR = 3,
        NDB = 4,
        USER = 5,
        ATC = 6
    }
}
