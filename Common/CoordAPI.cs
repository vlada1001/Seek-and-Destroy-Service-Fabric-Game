using Newtonsoft.Json;

namespace Common.API
{
    public class CoordAPI
    {
        [JsonProperty("X")]
        public int X { get; set; }
        [JsonProperty("Y")]
        public int Y { get; set; }
    }
}
