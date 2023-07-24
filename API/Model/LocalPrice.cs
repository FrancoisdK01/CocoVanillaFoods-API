using Newtonsoft.Json;

namespace API.Model
{
    public class LocalPrice
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}
