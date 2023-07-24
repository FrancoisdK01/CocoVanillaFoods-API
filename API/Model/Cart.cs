using API.Model;
using System.Text.Json.Serialization;

public class Cart
{
    public int CartID { get; set; }
    public string CustomerID { get; set; }
    public ICollection<CartItem> CartItems { get; set; }

    public int CartTotal
    {
        get
        {
            return (int)(CartItems != null ? CartItems.Sum(item => item.Quantity * item.Wine.WinePrice) : 0);
        }
    }

    [JsonIgnore]
    public Customer Customer { get; set; }
}
