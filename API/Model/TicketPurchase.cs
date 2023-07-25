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
    }
}
