using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization; // This namespace is used for JSON serialization.

namespace API.Model
{
    public class Inventory
    {
        [Key]
        public int InventoryID { get; set; }

        public int StockLimit { get; set; }
        public int QuantityOnHand { get; set; }

        [ForeignKey("WineID")]
        public int WineID { get; set; }

        public Wine Wine { get; set; }

        public int WineTypeID { get; set; }
        public int VarietalID { get; set; }

        [JsonIgnore]
        public virtual WineType WineType { get; set; }

        [JsonIgnore]
        public virtual Varietal Varietal { get; set; }

        public decimal WinePrice { get; set; } // Change the data type to decimal for the price.

        [JsonIgnore]
        public List<StockTake_Item> StockTake_Items { get; set; }






        //[Key] // This attribute marks the property as the primary key for the table.
        //public int InventoryID { get; set; } // Represents the unique identifier for each inventory item.

        //public int StockLimit { get; set; } // Represents the maximum allowed stock quantity for the inventory item.

        //public int QuantityOnHand { get; set; } // Represents the current quantity of the item available in stock.

        //[ForeignKey("Id")]
        //public int WineID { get; set; }

        //public Wine Name { get; set; } // Represents the name of the wine.

        //public int WineTypeID { get; set; } // Foreign key for WineType

        //public int VarietalID { get; set; } // Foreign key for Varietal

        //public virtual WineType WineType { get; set; } // Reference to WineType entity

        //public virtual Varietal Varietal { get; set; } // Reference to Varietal entity

        //public Wine WinePrice { get; set; } // Represents the price of the wine.
        //// The following two properties are commented out, but they were likely used to establish a foreign key relationship with the Wine table.
        //// A foreign key is used to associate an inventory item with a specific wine.

        ////[ForeignKey("WineID")] // This attribute specifies that this property is a foreign key for the WineID property in the Wine table.
        ////public int? WineID { get; set; } // Represents the foreign key to associate an inventory item with a specific wine.

        ////public Wine Wine { get; set; } // Represents the navigation property used to access the associated Wine object.

        //[JsonIgnore] // This attribute tells the JSON serializer to ignore this property when serializing the object to JSON.
        //public List<StockTake_Item> StockTake_Items { get; set; } // Represents a collection of stock take items associated with this inventory item.
        //// StockTake_Item is likely another class that represents a stock take entry for this specific inventory item.
    }
}
