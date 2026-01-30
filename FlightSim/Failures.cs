using System;
using System.Collections.Generic;
using System.Text;

namespace FlightSim
{
    public enum Failures
    {
        AlwaysWorking = 0,
        MeanTimeUntilFailure = 1,
        ExactTimeUntilFailure = 2,
        FailAtExactSpeedKIAS = 3,
        FailAtExactAltitudeAGL = 4,
        FailIfCTRLFOrJOY = 5,
        Inoperative = 6
    }
}
