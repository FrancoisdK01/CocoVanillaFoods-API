using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Discount
    {
        [Key]
        public int DiscountID { get; set; }

        [MaxLength(50)]
        public string DiscountCode { get; set; }

        [MaxLength(50)]
        public string DiscountDescription { get; set; }

        public double DiscountAmount { get; set; }

    }
}
