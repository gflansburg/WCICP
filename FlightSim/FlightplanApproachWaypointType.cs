using System;
using System.Collections.Generic;
using System.Text;

namespace FlightSim
{
    public enum FlightPlanApproachWaypointType
    {
        None = 0,
        Fix = 1,
        ProcTurnLeft = 2,
        ProcTurnRight = 3,
        DmeArcLeft = 4,
        DmeArcRight = 5,
        HoldingLeft = 6,
        HoldingRight = 7,
        Distance = 8,
        Altitude = 9,
        ManualSeq = 10,
        VectorsToFinal = 11
    }
}
