namespace FlightSim
{
    public class FlightSimProviders
    {
        public static readonly SimConnectProvider SimConnect = SimConnectProvider.Instance;
        public static readonly FSUIPCProvider FSUIPC = FSUIPCProvider.Instance;
        public static readonly XPlaneProvider XPlane = XPlaneProvider.Instance;
        public static readonly DCSProvider? DCS = DCSProvider.Instance;
        public static readonly FalconBMSProvider FalcomBMS = FalconBMSProvider.Instance;
        public static readonly PreviewFlightSimProvider Preview = PreviewFlightSimProvider.Instance;
    }
}
