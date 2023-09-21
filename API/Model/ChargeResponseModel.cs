using Newtonsoft.Json;

namespace API.Model
{
    public class ChargeResponseModel
    {
        [JsonProperty("data")]
        public ChargeDataResponseModel Data { get; set; }
    }
}
