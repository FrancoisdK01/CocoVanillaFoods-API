using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class TicketPurchase
    {
        [Key]
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public int EventId { get; set; }
        public DateTime EventDate { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TicketPrice { get; set; }
        public string EventName { get; set; }  // New field
        public string Description { get; set; }

        public virtual TicketPurchasedStatus TicketPurchasedStatus { get; set; }


        public int QrId { get; set; }

        // Navigation property for one-to-one relationship with QrCode
        [ForeignKey("QrId")]
        public virtual QrCode QrCode { get; set; }

        public virtual Event Event { get; set; }

        public string CustomerId { get; set; } // Change type to string

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

    }
}
