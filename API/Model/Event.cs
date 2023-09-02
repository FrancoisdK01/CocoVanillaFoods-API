using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace API.Model
{
    public class Event : ShopItem
    {

        [Key]
        public int EventID { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public DateTime EventDate { get; set; }
        public int Tickets_Available { get; set; }
        public int Tickets_Sold { get; set; }

        public int? EarlyBirdID { get; set; }
        [ForeignKey("EarlyBirdID")]
        public EarlyBird EarlyBird { get; set; }
        // Other specific properties

        // Adding EventType relationship
        public int? EventTypeID { get; set; }
        [ForeignKey("EventTypeID")]
        public EventType EventType { get; set; }


    }
}
