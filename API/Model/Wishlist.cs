using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Wishlist
    {
        [Key]
        public int WishlistID { get; set; }

        [ForeignKey("CustomerID")]
        public string CustomerID { get; set; }
        public Customer Customer { get; set; }

        [JsonIgnore]
        public List<WishlistItem> WishlistItems { get; set; }
    }
}
