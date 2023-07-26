using System.Text.Json.Serialization;

namespace API.Model
{
    public class WishlistItem
    {
        public int WishlistItemID { get; set; }
        public int WineID { get; set; }

        public int WishlistID { get; set; } // Added this line

        [JsonIgnore]
        public Wine Wine { get; set; }

        [JsonIgnore]
        public Wishlist Wishlist { get; set; }
    }
}