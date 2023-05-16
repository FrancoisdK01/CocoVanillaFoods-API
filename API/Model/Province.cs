using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    public class Province
    {
        [Key]
        public int ProvinceID { get; set; }

        [MaxLength(50)]
        public string Description { get; set; }

        public DateTime Date_of_last_update { get; set; }
    }
}
