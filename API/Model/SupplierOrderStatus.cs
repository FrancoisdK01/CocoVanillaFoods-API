using API.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class SupplierOrderStatus
{
    [Key]
    public int SupplierOrderStatusID { get; set; }

    [ForeignKey("SupplierOrderID")]
    public int SupplierOrderID { get; set; }
    public virtual SupplierOrder SupplierOrder { get; set; }

    public bool Ordered { get; set; } = false;

    public bool Paid { get; set; } = false;

    public bool Received { get; set; } = false;
}