    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class TicketPurchasedStatus
    {
        [Key]

        public int Id { get; set; } // Primary key
        

        public bool EventDeleted { get; set; }
        public bool IsScanned { get; set; }
        public DateTime? ScannedAt { get; set; }
        public string ScanningToken { get; set; } // Unique token for each ticket

        [ForeignKey("TicketPurchase")]
        public int TicketPurchaseId { get; set; } // Foreign key

        [JsonIgnore]
        // Navigation property to TicketPurchase
        public virtual TicketPurchase TicketPurchase { get; set; }
    }
}
