using System.Text.Json.Serialization;

namespace API.Model
{
    public class RefundItem
    {
        public int RefundItemId { get; set; }

        public int RefundRequestId { get; set; } // Foreign Key referencing RefundRequest

        public int WineOrderItemId { get; set; } // Foreign Key referencing WineOrderItem

        public int Quantity { get; set; } // Quantity of the wine to be refunded
        public string Reason { get; set; }

        [JsonIgnore]
        public RefundRequest RefundRequest { get; set; }

        [JsonIgnore]
        public WineOrderItem WineOrderItem { get; set; }
    }
}
