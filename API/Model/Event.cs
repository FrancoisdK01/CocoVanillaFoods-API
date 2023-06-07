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

        public int EventPrice { get; set; }


        //public int? EventPriceID { get; set; }

        //public int? EarlyBirdID { get; set; }

        //public int? EventTypeID { get; set; }

        //public virtual EventPrice EventPrice { get; set; }

        //public virtual EarlyBird EarlyBird { get; set; }

        //public virtual EventType EventType { get; set; }



        

        [JsonIgnore]
        public List<Booking> Bookings { get; set; }



    }
}
