using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace FlightSim
{
    public class DCSCrossref
    {
        public string AircraftName { get; set; } = null!;
        public int AircraftId { get; set; }

        public static List<DCSCrossref> GetDCSWorldCrossref()
        {
            try
            {
                RestClient client = new RestClient("https://cloud.gafware.com/Home");
                RestRequest request = new RestRequest("GetDCSWorldCrossref", Method.Get);
                RestResponse response = client.Execute(request);
                if (response != null && response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    var list = JsonConvert.DeserializeObject<List<DCSCrossref>>(response.Content);
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
            return new List<DCSCrossref>()
            {
                /*new DCSWorldCrossref()
                {

                }*/
            };
        }
    }
}