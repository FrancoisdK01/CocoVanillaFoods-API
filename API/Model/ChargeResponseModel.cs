using Newtonsoft.Json;

namespace API.Model
{
    public class ChargeResponseModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("logo_url")]
        public string LogoUrl { get; set; }

        [JsonProperty("hosted_url")]
        public string HostedUrl { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; }

        [JsonProperty("timeline")]
        public string Timeline { get; set; }

        [JsonProperty("metadata")]
        public Metadata Metadata { get; set; }

        [JsonProperty("pricing")]
        public Pricing Pricing { get; set; }

        [JsonProperty("payments")]
        public Payments Payments { get; set; }

        [JsonProperty("addresses")]
        public Addresses Addresses { get; set; }
    }
}
