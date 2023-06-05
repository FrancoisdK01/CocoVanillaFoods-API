using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


namespace API.Model
{
    public class Event
    {

        [Key]
        public int EventID { get; set; }

        [MaxLength(255)]
        public string EventName { get; set; }

        public DateTime EventDate { get; set; }

        public int Tickets_Available { get; set; }

        public int Tickets_Sold { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]

        //public string Image_URL { get; set; }

        //public double EventPrice { get; set; }

        public int EventPriceID { get; set; }
        public int EventTypeID { get; set; }
        public int EarlyBirdID { get; set; }
        public virtual EventPrice EventPrice { get; set; }
        public virtual EventType EventType { get; set; }
        public virtual EarlyBird EarlyBird { get; set; }

        //[ForeignKey("EventPriceID")]

        //[ForeignKey("EventTypeID")]

        //[ForeignKey("EarlyBirdID")]

        //[ForeignKey("EventLocationID")]

        //public int EventLocationID { get; set; }
        //public virtual EventLocation EventLocation { get; set; }


        [JsonIgnore]
        public List<Booking> Bookings { get; set; }



    }
}
