using System.Text.Json.Serialization;

namespace API.Model
{
    public class RefundRequest
    {
        public int RefundRequestId { get; set; }

        public int WineOrderId { get; set; } // Foreign Key referencing WineOrder

        public DateTime RequestDate { get; set; } = DateTime.Now;

        public string Status { get; set; } // To check if the refund request is approved

        [JsonIgnore]
        public WineOrder WineOrder { get; set; }

        public virtual ICollection<RefundItem> RefundItems { get; set; }

    }
}
