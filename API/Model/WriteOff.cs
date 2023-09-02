using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class WriteOff
    {
        [Key]
        public int WriteOffID { get; set; }

        public int WriteOff_ReasonID { get; set; }

        [JsonIgnore]
        public virtual WriteOff_Reason WriteOff_Reason { get; set; }

        public DateTime WriteOff_Date { get; set; }

        public int InventoryID { get; set; }

        [JsonIgnore]
        public virtual Inventory Inventory { get; set; }

        public int Quantity { get; set; }
    }
}
