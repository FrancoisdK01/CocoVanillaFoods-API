using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class RefundType
    {
        [Key]
        public int RefundTypeID { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

    }
}
