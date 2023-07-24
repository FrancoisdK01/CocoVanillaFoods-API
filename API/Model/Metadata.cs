using Newtonsoft.Json;

namespace API.Model
{
    public class Metadata
    {
        [JsonProperty("customer_id")]
        public string CustomerId { get; set; }

        [JsonProperty("customer_name")]
        public string CustomerName { get; set; }
    }
}
