using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Varietal : Types
    {
        [Key]
        public int VarietalID { get; set; }


        // Foreign key for WineType
        [ForeignKey("WineTypeID")]
        public int WineTypeID { get; set; }

        // Navigation properties
        public virtual WineType WineType { get; set; }

        public bool Blend { get; set; }

        [JsonIgnore]
        public List<Wine> Wines { get; set; }
    }
}
