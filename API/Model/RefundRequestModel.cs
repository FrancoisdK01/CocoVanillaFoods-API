namespace API.Model
{
    public class RefundRequestModel
    {
        public int WineId { get; set; }
        public string Email { get; set; }
        public decimal Cost { get; set; }
        public string Description { get; set; } // New field
        public string ReferenceNumber { get; set; }
        public string PhoneNumber { get; set; }
    }
}
