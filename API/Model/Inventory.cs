using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // This namespace is used for JSON serialization.

namespace API.Model
{
    public class Inventory
    {
        [Key] // This attribute marks the property as the primary key for the table.
        public int InventoryID { get; set; } // Represents the unique identifier for each inventory item.

        public int StockLimit { get; set; } // Represents the maximum allowed stock quantity for the inventory item.

        public int QuantityOnHand { get; set; } // Represents the current quantity of the item available in stock.

        public string WineName { get; set; } // Represents the name of the wine.

        public string WineVarietal { get; set; } // Represents the varietal or grape type of the wine.

        public string WineType { get; set; } // Represents the type or category of the wine.

        public double winePrice { get; set; } // Represents the price of the wine.

        // The following two properties are commented out, but they were likely used to establish a foreign key relationship with the Wine table.
        // A foreign key is used to associate an inventory item with a specific wine.

        //[ForeignKey("WineID")] // This attribute specifies that this property is a foreign key for the WineID property in the Wine table.
        //public int? WineID { get; set; } // Represents the foreign key to associate an inventory item with a specific wine.

        //public Wine Wine { get; set; } // Represents the navigation property used to access the associated Wine object.

        [JsonIgnore] // This attribute tells the JSON serializer to ignore this property when serializing the object to JSON.
        public List<StockTake_Item> StockTake_Items { get; set; } // Represents a collection of stock take items associated with this inventory item.
        // StockTake_Item is likely another class that represents a stock take entry for this specific inventory item.
    }
}
