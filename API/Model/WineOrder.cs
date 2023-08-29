using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using API.Model;

public class WineOrder
{
    public int WineOrderId { get; set; } // This will be automatically configured as the primary key

    public string CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime CollectedDate { get; set; }

    public int OrderTotal { get; set; }

    // The following line is commented out to be replaced by a foreign key
    // public int OrderStatus { get; set; } 

    [ForeignKey("OrderStatus")]
    public int OrderStatusId { get; set; }  // Foreign Key referencing OrderStatus

    public virtual OrderStatus OrderStatus { get; set; } // Navigation property

    public string OrderRefNum { get; set; }

    public bool isRefunded { get; set; }

    [JsonIgnore]
    public Customer Customer { get; set; }

    public List<WineOrderItem> OrderItems { get; set; }

    public virtual ICollection<RefundRequest> RefundRequests { get; set; }
}