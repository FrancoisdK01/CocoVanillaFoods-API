using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class EarlyBird
    {
        public int EarlyBirdID { get; set; }

        public int Percentage { get; set; }

        [Range(0, 999)]
        public int Limit { get; set; }

        [JsonIgnore]
        public ICollection<Event> Events { get; set; }
    }
}
