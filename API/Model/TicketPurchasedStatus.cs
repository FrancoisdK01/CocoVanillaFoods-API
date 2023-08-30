namespace API.Model
{
    public class TicketPurchasedStatus
    {
        public int Id { get; set; } // Primary key
        

        public bool EventDeleted { get; set; }
        public bool IsScanned { get; set; }
        public DateTime? ScannedAt { get; set; }
        public string ScanningToken { get; set; } // Unique token for each ticket


        public int TicketPurchaseId { get; set; } // Foreign key

        // Navigation property to TicketPurchase
        public virtual TicketPurchase TicketPurchase { get; set; }
    }
}
