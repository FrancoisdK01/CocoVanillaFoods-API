using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [JsonIgnore]
        public List<SupplierOrder> SupplierOrders { get; set; }
    }
}
