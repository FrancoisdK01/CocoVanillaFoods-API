using API.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Wishlist
{
    public int WishlistID { get; set; }
    public string CustomerID { get; set; }
    public ICollection<WishlistItem> WishlistItems { get; set; }

    [JsonIgnore]
    public Customer Customer { get; set; }
}