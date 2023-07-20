namespace API.Model
{
    public class PayFastResponse
    {
        public decimal amount { get; set; } // The amount of the payment
        public string currency { get; set; } // The currency of the payment
        public string customer_id { get; set; } // The ID of the customer
        public string item_description { get; set; } // The description of the item
        public string item_name { get; set; } // The name of the item
        public string message { get; set; } // A message about the payment status
        public string order_id { get; set; } // The ID of the order
        public string payment_method { get; set; } // The payment method used
        public string payment_status { get; set; }
        public string pf_payment_id { get; set; }
        public string transaction_id { get; set; } // The ID of the transaction
        public DateTime timestamp { get; set; }
    }
}
