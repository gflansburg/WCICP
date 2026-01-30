using System;
using System.Collections.Generic;
using System.Text;

namespace FlightSim
{
    public enum ComStatus
    {
        Invalid = -1,
        OK = 0,
        DoesNotExist = 1,
        NoElectricity = 2,
        Failed = 3
    }
}
