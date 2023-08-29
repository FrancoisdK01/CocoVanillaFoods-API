using API.Model;
using System.Text.Json.Serialization;

public class Cart
{
    public int CartID { get; set; }
    public string CustomerID { get; set; }
    public ICollection<CartItem> CartItems { get; set; }

    public double DiscountedCart { get; set; }

    public double CartTotal
    {
        get
        {
            if (DiscountedCart != 0)
            {
                return DiscountedCart;
            }
            return (int)(CartItems != null ? CartItems.Sum(item => item.Quantity * item.Wine.Price) : 0);
        }
    }


    [JsonIgnore]
    public Customer Customer { get; set; }
}
