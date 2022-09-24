using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PredictStarNumberMod
{
    internal static class StarNumberPredictor
    {
        internal static string hash;
        internal static string mapType;
        
        public static async Task<string> PredictStarNumber()
        {
            string endpoint = $"https://predictstarnumber.herokuapp.com/api2/hash/{hash}";

            HttpClient client = new HttpClient();
            var response = await client.GetAsync(endpoint);
            string jsonString = await response.Content.ReadAsStringAsync();
            
            dynamic jsonDynamic = JsonConvert.DeserializeObject<dynamic>(jsonString);

            string rank = JsonConvert.SerializeObject(jsonDynamic[mapType]);

            return rank;
        }
    }
}
