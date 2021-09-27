using Newtonsoft.Json;
using System;

namespace WebAPI.Models
{
    public class PlayerAPI
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("coordinates")]
        public CoordAPI Coordinates { get; set; }

        [JsonProperty("hp")]
        public int HP { get; set; }

        [JsonProperty("ad")]
        public int AD { get; set; }

        [JsonProperty("numberOfFights")]
        public int NumberOfFights { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("opponent")]
        public string Opponent { get; set; }
    }
}
