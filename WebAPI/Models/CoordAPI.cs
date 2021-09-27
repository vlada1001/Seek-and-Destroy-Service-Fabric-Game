using Newtonsoft.Json;

namespace WebAPI.Models
{
    public class CoordAPI
    {
        [JsonProperty("X")]
        public int X { get; set; }
        [JsonProperty("Y")]
        public int Y { get; set; }
    }
}
