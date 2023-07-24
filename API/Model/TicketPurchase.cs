namespace API.Model
{
    public class TicketPurchase
    {
        public int Id { get; set; }

        // Updated foreign key property name
        public int EventID { get; set; }
        public Event Event { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public DateTime PurchaseDate { get; set; }
        public decimal PricePaid { get; set; }
        public bool EarlyBirdDiscountApplied { get; set; }
    }
}
