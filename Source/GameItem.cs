using Newtonsoft.Json;

namespace Parser
{
    public class GameItem
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "inventory_width")]
        public int SizeX { get; set; }

        [JsonProperty(PropertyName = "inventory_height")]
        public int SizeY { get; set; }
    }
}
