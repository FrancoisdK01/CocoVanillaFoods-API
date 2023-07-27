using API.Model;
using System.Text.Json.Serialization;

public class WineOrder
{
    public int WineOrderId { get; set; } // This will be automatically configured as the primary key

    public string CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

  

    [JsonIgnore]
    public Customer Customer { get; set; }

    public List<WineOrderItem> OrderItems { get; set; }

    //public virtual ICollection<WineOrderItem> OrderItems { get; set; }
    //public virtual ICollection<WineOrderItem> OrderItems { get; set; }

}
