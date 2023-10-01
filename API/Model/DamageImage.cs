using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class DamageImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FilePath { get; set; }

        public int? EventID { get; set; }

        [ForeignKey("EventID")]
        public Event Event { get; set; }
    }
}
