﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class OrderItem
    {
        [Key]
        public int OrderItemID { get; set; }

        public int Quantity { get; set; }

        [ForeignKey("WineID")]
        public int WineID { get; set; }
        public Wine Wine { get; set; }

        [ForeignKey("OrderID")]
        public int OrderID { get; set; }
        public Order Order { get; set; }

        public List<Refund> Refunds { get; set; }
    }
}
