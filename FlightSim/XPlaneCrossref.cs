using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace FlightSim
{
    public class XPlaneCrossref
    {
        public string AircraftName { get; set; } = null!;
        public int AircraftId { get; set; }

        public static List<XPlaneCrossref> GetXPlaneCrossref()
        {
            try
            {
                RestClient client = new RestClient("https://cloud.gafware.com/Home");
                RestRequest request = new RestRequest("GetXPlaneCrossref", Method.Get);
                RestResponse response = client.Execute(request);
                if (response != null && response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    List<XPlaneCrossref>? crossrefs = JsonConvert.DeserializeObject<List<XPlaneCrossref>>(response.Content);
                    return crossrefs != null ? crossrefs : new List<XPlaneCrossref>();
                }
            }
            catch (Exception)
            {
                // Offline?
            }
            return new List<XPlaneCrossref>()
            {
                /*new XPlaneCrossref()
                {

                }*/
            };
        }
    }
}