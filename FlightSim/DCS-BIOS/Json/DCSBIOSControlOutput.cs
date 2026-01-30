using FlightSim.DCS_BIOS.Serialized;
using Newtonsoft.Json;

namespace FlightSim.DCS_BIOS.Json
{
    public class DCSBIOSControlOutput
    {
        private string _type = null!;

        [JsonProperty("address", Required = Required.Default)]
        public ushort Address { get; set; }

        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; } = null!;

        [JsonProperty("mask", Required = Required.Default)]
        public ushort Mask { get; set; }

        [JsonProperty("max_value", Required = Required.Default)]
        public ushort MaxValue { get; set; }

        [JsonProperty("shift_by", Required = Required.Default)]
        public ushort ShiftBy { get; set; }

        [JsonProperty("suffix", Required = Required.Default)]
        public string Suffix { get; set; } = null!;

        [JsonProperty("max_length", Required = Required.Default)]
        public ushort MaxLength { get; set; }

        [JsonProperty("address_identifier", Required = Required.Default)]
        public string AddressIdentifier { get; set; } = null!;

        [JsonProperty("address_mask_identifier", Required = Required.Default)]
        public string AddressMaskIdentifier { get; set; } = null!;

        [JsonProperty("address_mask_shift_identifier", Required = Required.Default)]
        public string AddressMaskShiftIdentifier { get; set; } = null!;

        [JsonProperty("type", Required = Required.Default)]
        public string Type
        {
            get => _type; 
            set
            {
                _type = value;
                OutputDataType = _type switch
                {
                    "string" => DCSBiosOutputType.StringType,
                    "integer" => DCSBiosOutputType.IntegerType,
                    _ => DCSBiosOutputType.None
                };
            }
        }

        public DCSBiosOutputType OutputDataType { get; private set; }
    }
}
