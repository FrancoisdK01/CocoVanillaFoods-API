
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Varietal : Types
    {
        [Key]
        public int VarietalID { get; set; }  

        [JsonIgnore]
        public List<Wine> Wines { get; set; }
    }
}
