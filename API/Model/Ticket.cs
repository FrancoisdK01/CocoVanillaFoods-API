using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class Ticket
    {
        internal readonly object Booking;

        [Key]
        public int TicketID { get; set; }

        public double TicketPrice { get; set; }

        public DateTime PurchaseDate { get; set; }

        public DateTime ExpiredDate { get; set; }

        public string QRCode { get; set; }

        public string CustomerID { get; set; }
        public Customer Customer { get; set; }

        //public int? BookingId { get; set; }

    }
}
