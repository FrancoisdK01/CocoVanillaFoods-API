namespace API.Model
{
    public class TicketPurchase
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public int EventId { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TicketPrice { get; set; }
        public string EventName { get; set; }  // New field
        public string Description { get; set; }

        public bool EventDeleted { get; set; }
        public bool IsScanned { get; set; }
        public DateTime? ScannedAt { get; set; }
        public string ScanningToken { get; set; }  // Unique token for each ticket

    }
}
