using System.Text.Json.Serialization;

namespace API.Model
{
    public enum RefundStatus
    {
        InProgress,
        Approved,
        NotApproved
    }

    public class RefundRequest
    {
        public int Id { get; set; }
        public int WineId { get; set; }
        public int OrderId { get; set; }
        public string Email { get; set; }
        public DateTime RequestedOn { get; set; }
        public decimal Cost { get; set; }
        public string Description { get; set; } // New field
        public RefundStatus Status { get; set; }
        public string OrderRefNum { get; set; }

        public Boolean isRefunded { get; set; }

        [JsonIgnore]
        public WineOrder WineOrder { get; set; }

    }
}
