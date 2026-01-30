using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FlightSim
{
    [XmlRoot(ElementName = "SimBase.Document")]
    public class SimBaseDocument
    {
        [XmlAttribute(AttributeName = "Type")]
        public string Type { get; set; } = "Launch";

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; } = "1,0";

        [XmlElement(ElementName = "Descr")]
        public string Descr { get; set; } = null!;

        [XmlElement(ElementName = "Filename")]
        public string Filename { get; set; } = null!;

        [XmlElement(ElementName = "Disabled")]
        public SafeBool Disabled { get; set; }

        [XmlElement(ElementName = "Launch.ManualLoad")]
        public SafeBool LaunchManualLoad { get; set; }

        [XmlElement(ElementName = "Launch.Addon")]
        public List<LaunchAddon> LaunchAddons { get; set; }

        public SimBaseDocument()
        {
            LaunchAddons = new List<LaunchAddon>();
        }
    }
}
