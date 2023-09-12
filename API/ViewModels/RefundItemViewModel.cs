namespace API.ViewModels
{
    public class RefundItemViewModel
    {
        public int RefundItemId { get; set; }
        public int RefundRequestId { get; set; }
        public int WineOrderItemId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
    }
}
