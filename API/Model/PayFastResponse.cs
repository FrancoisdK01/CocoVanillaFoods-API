namespace API.Model
{
    public class PayFastResponse
    {
        public decimal amount { get; set; } // The amount of the paymentus
        public string order_id { get; set; } // The ID of the order
        public string pf_payment_id { get; set; }
        public string transaction_id { get; set; } // The ID of the transaction
        public DateTime timestamp { get; set; }
    }
}
