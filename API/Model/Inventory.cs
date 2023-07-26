using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Model
{
    public class Inventory
    {
        [Key]
        public int InventoryID { get; set; }

        public int StockLimit { get; set; }

        public int QuantityOnHand { get; set; }

        public string WineName { get; set; }

        public string WineVarietal { get; set; }

        public string WineType { get; set; }

        public double winePrice { get; set; }

        //[ForeignKey("WineID")]
        //public int? WineID { get; set; }
        //public Wine Wine { get; set; }

        [JsonIgnore]
        public List<StockTake_Item> StockTake_Items { get; set; }

    }
}
