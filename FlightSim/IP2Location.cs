using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightSim
{
    public class IP2Location
    {
        //public byte[] ip_From { get; set; }
        //public byte[] ip_to { get; set; }
        public long ip_From { get; set; }
        public long ip_to { get; set; }
        public string? countrycode { get; set; }
        public string? country { get; set; }
        public string? regioncode { get; set; }
        public string? region { get; set; }
        public string? city { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string? timezone { get; set; }
        public double? elevation { get; set; }
        public bool dst { get; set; }
        public string? tailprefix { get; set; }
    }
}
