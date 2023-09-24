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

        public double OrderTotal { get; set; }

        [ForeignKey("SupplierID")]
        public int SupplierID { get; set; }
        public virtual Supplier Supplier { get; set; }

        [ForeignKey("InventoryID")]
        public int InventoryID { get; set; } // Foreign Key for Inventory
        public virtual Inventory Inventory { get; set; } // Navigation property for Inventory

        public Boolean isBackOrder { get; set; } = false;

        // Foreign key to the SupplierOrderStatus table
        public virtual SupplierOrderStatus SupplierOrderStatus { get; set; }
    }
}