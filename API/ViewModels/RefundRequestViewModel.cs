namespace API.ViewModels
{
    public class RefundRequestViewModel
    {
        public int RefundRequestId { get; set; }
        public int WineOrderId { get; set; }
        public DateTime RequestDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public List<RefundItemViewModel> RefundItems { get; set; }
    }
}
