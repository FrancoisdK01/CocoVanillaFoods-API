using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class EventPayments
    {
        [Key]
        public int PaymentId { get; set; }
        public int merchant_id { get; set; }
        public string merchant_key { get; set; }
        public int amount { get; set; }
        public string item_name { get; set; }
        public string signature { get; set; }
        public string email_address { get; set; }
        public string cell_number { get; set; }
    }
}
