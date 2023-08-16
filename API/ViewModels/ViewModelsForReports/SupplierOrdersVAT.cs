using API.Model;

namespace API.ViewModels.ViewModelsForReports
{
    public class SupplierOrdersVAT
    {
        public List<SupplierOrder> SupplierOrders { get; set; }
        public VAT VATs { get; set; }
    }
}
