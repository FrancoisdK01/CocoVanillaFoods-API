using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Wine : ShopItem
    {
        [Key]
        public int WineID { get; set; }

        [StringLength(50)]
        public string Vintage { get; set; }

        //[Range(0, 999)]
        //public int RestockLimit { get; set; }

        [StringLength(255)]
        public string WineTastingNote { get; set; }

        public int WineTypeID { get; set; } // Foreign key for WineType
        public int VarietalID { get; set; } // Foreign key for Varietal

        // Navigation properties
        public virtual WineType WineType { get; set; } // Reference to WineType entity
        public virtual Varietal Varietal { get; set; } // Reference to Varietal entity

        [JsonIgnore]
        public List<Inventory> Inventories { get; set; }

        [JsonIgnore]
        public List<SupplierOrder> SupplierOrders { get; set; }

        //[JsonIgnore]
        //public List<WriteOffItem> WriteOffItems { get; set; }

        [JsonIgnore]
        public List<WishlistItem> WishlistItems { get; set; }

        [JsonIgnore]
        public List<CartItem> CartItems { get; set; }

        [JsonIgnore]
        public List<WinePrice> WinePrice { get; set; }
    }
}
