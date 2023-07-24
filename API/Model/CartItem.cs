using System.Text.Json.Serialization;

namespace API.Model
{
    public class CartItem
    {
        public int CartItemID { get; set; }
        public int CartID { get; set; }
        public int WineID { get; set; }
        public int Quantity { get; set; } = 1;

        [JsonIgnore]
        public Wine Wine { get; set; }


        [JsonIgnore]
        public Cart Cart { get; set; }
    }
}
