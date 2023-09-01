using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class QrCode
    {
        [Key]
        public int QrId { get; set; }

        public string QrCodeBase64 { get; set; }  // Add this line
        public int TicketPurchaseId { get; set; }

        [ForeignKey("TicketPurchaseId")]
        public TicketPurchase TicketPurchase { get; set; }


    }
}
