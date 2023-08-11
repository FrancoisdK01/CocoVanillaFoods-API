using System.Text.Json.Serialization;
using API.Model;

public class WineOrderItem
{
    public int WineOrderItemId { get; set; }

    public int OrderId { get; set; } // <- make sure this is here

    public int WineId { get; set; }

    public int Quantity { get; set; } = 1;

    public Wine Wine { get; set; }

    [JsonIgnore]
    public WineOrder WineOrder { get; set; }
}
