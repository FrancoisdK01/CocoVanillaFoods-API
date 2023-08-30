using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class SupplierOrder
    {
        [Key]
        public int SupplierOrderID { get; set; }

        public int Quantity_Ordered { get; set; }

        public DateTime DateOrdered { get; set; }

        public string WineName { get; set; }

        public string WineYear { get; set; }

        public string WineType { get; set; }

        public int WinePrice { get; set; }

        public double OrderTotal { get; set; }

        [ForeignKey("SupplierID")]
        public int SupplierID { get; set; }
        public virtual Supplier Supplier { get; set; }

        // Foreign key to the SupplierOrderStatus table
        public virtual SupplierOrderStatus SupplierOrderStatus { get; set; }
    }
}