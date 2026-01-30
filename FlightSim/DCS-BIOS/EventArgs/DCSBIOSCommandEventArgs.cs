namespace FlightSim.DCS_BIOS.EventArgs
{
    public class DCSBIOSCommandEventArgs : System.EventArgs                 
    {
        public string Sender { get; init; } = null!;
        public string Command { get; init; } = null!;
    }
}
