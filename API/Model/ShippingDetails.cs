using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class ShippingDetails
    {
        [Key]
        public int ShippingID { get; set; }

        public DateTime ShippingDate { get; set; }

        [MaxLength(50)]
        public string TrackingNumber { get; set; }

        [ForeignKey("OrderID")]
        public int OrderID { get; set; }
        //public Order Order { get; set; }
    }
}
