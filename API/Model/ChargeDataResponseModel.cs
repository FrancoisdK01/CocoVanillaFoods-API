using Newtonsoft.Json;

namespace API.Model
{
    public class ChargeDataResponseModel
    {
        [JsonProperty("hosted_url")]
        public string HostedUrl { get; set; }
    }
}
