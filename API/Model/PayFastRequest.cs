using System.Text.Json.Serialization;

namespace API.Model
{
    public class PayFastRequest
    {
        public int merchant_id { get; set; }
        public string merchant_key { get; set; }
        public decimal amount { get; set; }
        public string item_name { get; set; }
        public string signature { get; set; }
        public string email_address { get; set; }
        public string cell_number { get; set; }
    }
}
