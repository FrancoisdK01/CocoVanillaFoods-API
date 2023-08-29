using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public enum OrderStatusEnum
{
    ClientOrderPlaced = 1,
    SupplierOrderPlaced,
    Received,
    Collected,
    // ... add other statuses here
}


public class OrderStatus
{
    [Key]
    public int OrderStatusId { get; set; }

    public string StatusName { get; set; }

    // This will hold the list of WineOrders with this status
    public virtual ICollection<WineOrder> WineOrders { get; set; }

    public static OrderStatus FromEnum(OrderStatusEnum statusEnum)
    {
        return new OrderStatus
        {
            OrderStatusId = (int)statusEnum,
            StatusName = statusEnum.ToString()
        };
    }
}
