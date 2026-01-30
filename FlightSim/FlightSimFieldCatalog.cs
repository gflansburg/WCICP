using System;
using System.Linq;
using System.Reflection;

namespace FlightSim
{
    public static class FlightSimFieldCatalog
    {
        public static Dictionary<string, FlightSimFieldAttribute> GetFields()
        {
            return typeof(FlightSimProviderBase)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => (Prop: p, Meta: p.GetCustomAttribute<FlightSimFieldAttribute>(inherit: true)))
                .Where(x => x.Meta != null)
                .OrderBy(x => x.Meta!.FriendlyName)
                .ToDictionary(
                    x => x.Prop.Name,   // PropertyName = stable key
                    x => x.Meta!        // Attribute metadata
                );
        }
    }
}
