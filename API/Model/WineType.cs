using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class WineType : Types
    {
        [Key]
        public int WineTypeID { get; set; }

        // Navigation properties
        [JsonIgnore]
        public List<Wine> Wines { get; set; }

        [JsonIgnore]
        public List<Varietal> Varietals { get; set; }
    }
}
