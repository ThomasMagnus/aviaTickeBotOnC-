using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace telegramBot
{
    internal sealed class Tickets
    {
        private string aviaToken = "3899b13ca2637bbb06562dbae5bd3146";
        private string APIUrl = "https://api.travelpayouts.com/aviasales/v3/prices_for_dates";
        private Dictionary<string, object> cityRoute;
        private Dictionary<string, object> responseData;
        public static string routeBack;
        public static string route;
        public static string routeDate;
        public Tickets(string path)
        {
            cityRoute = new Cities(path, route).GetCityDict();
        }

        private Dictionary<string, object> GetTickets()
        {
            Console.WriteLine(JsonConvert.SerializeObject(cityRoute));
            Dictionary<string, object> city_dict = (Dictionary<string, object>)cityRoute["cityFrom"];
            Dictionary<string, object> city_to_dict = (Dictionary<string, object>)cityRoute["cityTo"];

            var city_to = city_to_dict["code"];
            var city_from = city_dict["code"];

            if (routeBack == "Да")
            {

                string[] dates = routeDate.Split('-');
                string dateFrom = DateTime.Parse(dates[0]).ToString("yyyy-MM-dd");
                string dateTo = DateTime.Parse(dates[1]).ToString("yyyy-MM-dd");

                APIUrl += $"?origin={city_from}&destination={city_to}" +
                    $"&currency=rub&departure_at={dateFrom}&return_at=" +
                    $"{dateTo}&sorting=price&direct=true&limit=10&token={aviaToken}";

            } else if (routeBack == "Нет")
            {
                routeDate = DateTime.Parse(routeDate).ToString("yyyy-MM-dd");
                Console.WriteLine(routeDate);
                APIUrl += $"?origin={city_from}&destination={city_to}" +
                    $"&currency=rub&departure_at={routeDate}" +
                    $"&sorting=price&direct=true&limit=10&token={aviaToken}";
            }

            try
            {
                Console.WriteLine(APIUrl);
                WebRequest request = WebRequest.Create(APIUrl);
                WebResponse response = request.GetResponse();

                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return JsonConvert.DeserializeObject<Dictionary<string, object>>(reader.ReadToEnd());
                    }
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public Dictionary<string, object> ResponseData
        {
            get
            {
                responseData = GetTickets();
                return responseData;
            }
        }
    }


    public partial class TicketsData
    {
        [JsonProperty("origin")]
        public string Origin { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }

        [JsonProperty("origin_airport")]
        public string OriginAirport { get; set; }

        [JsonProperty("destination_airport")]
        public string DestinationAirport { get; set; }

        [JsonProperty("price")]
        public int Price { get; set; }

        [JsonProperty("airline")]
        public string Airline { get; set; }

        [JsonProperty("flight_number")]
        public int FlightNumber { get; set; }

        [JsonProperty("departure_at")]
        public DateTimeOffset DepartureAt { get; set; }

        [JsonProperty("return_at")]
        public DateTimeOffset ReturnAt { get; set; }

        [JsonProperty("transfers")]
        public long Transfers { get; set; }

        [JsonProperty("return_transfers")]
        public long ReturnTransfers { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }

    public partial class TicketsData
    {
        public static TicketsData[] FromJson(string json) => JsonConvert.DeserializeObject<TicketsData[]>(json);
    }
}
