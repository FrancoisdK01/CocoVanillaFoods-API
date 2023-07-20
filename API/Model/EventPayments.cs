using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class EventPayments
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public decimal PaymentAmount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        // Navigation properties
        [ForeignKey("EventId")]
        public Event Event { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
