using FlightSim.DCS_BIOS.Serialized;

namespace FlightSim.DCS_BIOS.StringClasses
{
    /// <summary>
    /// Easy wrapper for listening to DCS-BIOS strings.
    /// </summary>
    public static class DCSBIOSStringManager
    {
        private static DCSBIOSStringListener _dcsbiosStringListener = null!;

        private static void AddAddress(ushort address, ushort length)
        {
            CheckInstance();
            _dcsbiosStringListener.AddStringAddress(address, length);
        }

        public static void AddListeningAddress(DCSBIOSOutput dcsbiosOutput)
        {
            CheckInstance();
            AddAddress(dcsbiosOutput.Address, dcsbiosOutput.MaxLength);
        }

        private static void CheckInstance()
        {
            _dcsbiosStringListener ??= new DCSBIOSStringListener();
        }

        public static void Close()
        {
            _dcsbiosStringListener?.Dispose();
        }
    }
}
