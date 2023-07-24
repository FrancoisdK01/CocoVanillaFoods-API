namespace API.Model
{
    public class PayFastPaymentResult
    {
            public string payment_status { get; set; }
            public string m_payment_id { get; set; }
            public string pf_payment_id { get; set; }
            public string item_name { get; set; }
            public decimal amount_gross { get; set; }
            public decimal amount_fee { get; set; }
            public decimal amount_net { get; set; }
            public string custom_str1 { get; set; } // You can use this to pass additional data, like the customer email
            public string custom_str2 { get; set; } // You can use this to pass additional data, like the event ID
        // Add other properties as required
    }
}
