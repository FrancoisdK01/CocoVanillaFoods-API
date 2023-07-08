using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        public DateTime BookingDate { get; set; }

        [ForeignKey("EventID")]
        public int EventId { get; set; }
        public Event Event { get; set; }

        public List<Ticket> Tickets { get; set; }

        [JsonIgnore]
        public BookingPayment BookingPayment { get; set; }

        [ForeignKey("CustomerID")]
        public string CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
