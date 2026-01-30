using FlightSim.DCS_BIOS.Serialized;
using Newtonsoft.Json;

namespace FlightSim.DCS_BIOS.Json
{
    /// <summary>
    /// Used when reading the JSON to create list of the inputs
    /// that a DCS-BIOS Control has.
    /// </summary>
    public class DCSBIOSControlInput
    {
        public string ControlId { get; set; } = null!;

        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; } = null!;

        [JsonProperty("interface", Required = Required.Default)]
        public string ControlInterface { get; set; } = null!;

        [JsonProperty("max_value", Required = Required.Default)]
        public int? MaxValue { get; set; }

        [JsonProperty("suggested_step", Required = Required.Default)]
        public int? SuggestedStep { get; set; }

        [JsonProperty("argument", Required = Required.Default)]
        public string Argument { get; set; } = null!;

        public DCSBIOSInputInterface GetInputInterface()
        {
            var inputInterface = new DCSBIOSInputInterface();
            inputInterface.Consume(ControlId, this);
            return inputInterface;
        }
    }
}
