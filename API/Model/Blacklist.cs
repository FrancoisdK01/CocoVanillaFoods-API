using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Model
{
    public class Blacklist
    {
        [Key]
        public int BlacklistID { get; set; }

        [MaxLength(50)]
        public string Email { get; set; }

        [MaxLength(255)]
        public string Reason { get; set; }
    }
}
