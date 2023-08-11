using API.Model;
using System.Text.Json.Serialization;

public class WineOrder
{
    public int WineOrderId { get; set; } // This will be automatically configured as the primary key

    public string CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime CollectedDate { get; set; }

    public int OrderTotal { get; set; }

    public int OrderStatus { get; set; } // Renamed from Received to OrderStatus and type changed to int

    public string OrderRefNum { get; set; }

    public bool isRefunded { get; set; }

    [JsonIgnore]
    public Customer Customer { get; set; }

    public List<WineOrderItem> OrderItems { get; set; }
}
