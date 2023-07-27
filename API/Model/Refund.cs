using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class Refund
    {
        [Key]
        public int RefundID { get; set; }

        public int QuantityRefunded { get; set; }
        public DateTime DateIssued { get; set; }
        public DateTime? ResolvedDate { get; set; }

        [ForeignKey("OrderItemID")]
        public int OrderItemID { get; set; }
        public WineOrderItem OrderItem { get; set; }

        [ForeignKey("RefundResponseID")]
        public int RefundResponseID { get; set; }
        public RefundResponse RefundResponse { get; set; }

        [ForeignKey("RefundReasonID")]
        public int RefundReasonID { get; set; }
        public RefundReason RefundReason { get; set; }
    }
}
