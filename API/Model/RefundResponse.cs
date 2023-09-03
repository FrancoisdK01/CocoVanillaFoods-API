using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class RefundResponse
    {
        [Key]
        public int RefundResponseID { get; set; }

        [MaxLength(50)]
        public string ResponseValue { get; set; }

        [MaxLength(50)]
        public string Description { get; set; }

    }
}
