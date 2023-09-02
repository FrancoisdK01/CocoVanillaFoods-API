using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class EventType
    {
        [Key]
        public int EventTypeID { get; set; }

        [MaxLength(50)]
        public string EventTypeName { get; set; }


        [JsonIgnore]
        public List<Event> Events { get; set; }
    }
}
