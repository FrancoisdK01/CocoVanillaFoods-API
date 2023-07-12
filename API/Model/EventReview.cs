using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class EventReview
    {
        [Key]
        public int EventReviewID { get; set; }

        public int Rating { get; set; }

        [MaxLength(255)]
        public string Comment { get; set; }

        public DateTime Review_Date { get; set; }

        [ForeignKey("CustomerID")]
        public string CustomerID { get; set; }
        public Customer Customer { get; set; }
    }
}
