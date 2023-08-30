using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class OrderPayment
    {
        [Key]
        public int PaymentID { get; set; }

        public int AmountID { get; set; }

        public double Amount { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("OrderID")]
        public int OrderID { get; set; }
        public Order Order { get; set; }
    }
}
