using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;

namespace FlightSim
{
    public class FalconBMSCrossref
    {
        public string AircraftName { get; set; } = null!;
        public int AircraftId { get; set; }

        public static List<FalconBMSCrossref> GetFalconBMSCrossref()
        {
            try
            {
                RestClient client = new RestClient("https://cloud.gafware.com/Home");
                RestRequest request = new RestRequest("GetFalconBMSCrossref", Method.Get);
                RestResponse response = client.Execute(request);
                if (response != null && response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    List<FalconBMSCrossref>? list = JsonConvert.DeserializeObject<List<FalconBMSCrossref>>(response.Content);
                    if (list != null)
                    {
                        return list;
                    }
                }
            }
            catch (Exception)
            {
                // Offline?
            }
            return new List<FalconBMSCrossref>()
            {
                /*new FalconBMSCrossref()
                {

                }*/
            };
        }
    }
}