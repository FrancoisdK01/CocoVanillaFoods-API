using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    public class AuditTrail
    {
        [Key]
        public int AuditLogId { get; set; }
        public string ButtonPressed { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public DateTime TransactionDate{ get; set; }
        public int? Quantity { get; set; }
    }
}
