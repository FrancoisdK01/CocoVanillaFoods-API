using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class RefundResponse
    {
        [Key]
        public int RefundResponseID { get; set; }

        [MaxLength(8)]
        public string ResponseValue { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

    }
}
