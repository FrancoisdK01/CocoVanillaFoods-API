public class UpdateSupplierOrderStatusDTO
{
    public int SupplierOrderID { get; set; }
    public int SupplierOrderStatusID { get; set; }
    public bool Ordered { get; set; }
    public bool Paid { get; set; }
    public bool Received { get; set; }
}
