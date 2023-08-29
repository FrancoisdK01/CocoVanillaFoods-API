using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class WineType : Types
    {
        [Key]
        public int WineTypeID { get; set; }

        [JsonIgnore]
        public List<Wine> Wines { get; set; }
    }
}
