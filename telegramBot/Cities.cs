using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace telegramBot
{
    internal sealed class Cities
    {
        private string filePath, cityRoute;
        private Dictionary<string, object> citiesData = new Dictionary<string, object> { };


        public Cities(string filePath, string cityRoute)
        {
            this.filePath = filePath;
            this.cityRoute = cityRoute;
        }

        private async Task<List<Dictionary<string, object>>> GetCities()
        {
            using (StreamReader reader = new StreamReader(this.filePath))
            {
                string result = await reader.ReadToEndAsync();
                List<Dictionary<string, object>> citiesDict = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(result);
                return citiesDict;
            }
        }

        public Dictionary<string, object> GetCityDict()
        {
            string[] citiesRoutes = cityRoute.Split('/');
            List<Dictionary<string, object>> cities = GetCities().Result;
            IEnumerable<Dictionary<string, object>> cityFrom = cities.Where(x => x["name"] != null).Where(x => x["name"].ToString() == citiesRoutes[0]);
            IEnumerable<Dictionary<string, object>> cityTo = cities.Where(x => x["name"] != null).Where(x => x["name"].ToString() == citiesRoutes[1]);

            foreach (var item in cityFrom) citiesData["cityFrom"] = item;
            foreach (var item in cityTo) citiesData["cityTo"] = item;

            return citiesData;
        }
    }
}
