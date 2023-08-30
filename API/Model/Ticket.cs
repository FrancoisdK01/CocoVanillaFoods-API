using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class Ticket
    {
        public int TicketID { get; set; }
        public string CustomerEmail { get; set; }
        public int EventID { get; set; }
        public string PaymentID { get; set; }

    }
}
