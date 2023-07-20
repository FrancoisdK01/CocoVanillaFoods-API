namespace API.Model
{
    public class PayFastRequest
    {
        public int merchant_id { get; set; }
        public string merchant_key { get; set; }
        public int amount { get; set; }
        public string item_name { get; set; }
        public string signature { get; set; }
    }
}
