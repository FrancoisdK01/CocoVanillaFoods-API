using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text.Json.Serialization; // This namespace is used for JSON serialization.

namespace API.Model
{
    public class StockTake
    {
        [Key]
        public int stocktakeID { get; set; }

        public DateTime DateDone { get; set; }

        public int QuantityOrdered { get; set; }

        public int QuantityReceived { get; set; }

        public bool? Added { get; set; }
        
        [ForeignKey("SupplierOrder")]
        public int SupplierOrderID { get; set; }

        [JsonIgnore] // To avoid circular references during serialization
        public virtual SupplierOrder SupplierOrder { get; set; }

    }
}
