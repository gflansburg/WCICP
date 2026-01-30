using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FlightSim
{
	[XmlRoot(ElementName = "Launch.Addon")]
	public class LaunchAddon
	{

		[XmlElement(ElementName = "Disabled")]
		public SafeBool Disabled { get; set; }

		[XmlElement(ElementName = "ManualLoad")]
		public SafeBool ManualLoad { get; set; }

		[XmlElement(ElementName = "Name")]
		public string Name { get; set; } = null!;

		[XmlElement(ElementName = "Path")]
		public string Path { get; set; } = null!;

        [XmlElement(ElementName = "CommandLine")]
		public string CommandLine { get; set; } = null!;

        [XmlElement(ElementName = "NewConsole")]
		public SafeBool NewConsole { get; set; }
	}
}
