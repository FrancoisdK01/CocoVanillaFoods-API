using Newtonsoft.Json;

namespace API.Model
{
    public class ChargeModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("pricing_type")]
        public string PricingType { get; set; }

        [JsonProperty("local_price")]
        public LocalPrice LocalPrice { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty("cancel_url")]
        public string CancelUrl { get; set; }
    }
}
