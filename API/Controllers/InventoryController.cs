using API.Data;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly MyDbContext _context;

        public InventoryController(MyDbContext context)
        {
            _context = context; // Constructor that injects the MyDbContext dependency into the controller.
        }

        // GET: api/Inventory
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventory()
        {
            return await _context.Inventories.ToListAsync();
            // GET request to retrieve all inventory items from the database.
        }

        // GET: api/Inventory/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory>> GetInventory(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            // GET request to retrieve a specific inventory item by its ID from the database.

            if (inventory == null)
            {
                return NotFound();
                // If the requested inventory item is not found, return a 404 Not Found response.
            }

            return inventory;
            // Return the retrieved inventory item.
        }

        // PUT: api/Inventory/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInventory(int id, Inventory inventory)
        {
            var editInventory = _context.Inventories.FirstOrDefault(i => i.InventoryID == id);

            if(editInventory == null)
            {
                return NotFound();
            }          

            editInventory.QuantityOnHand = inventory.QuantityOnHand;
            editInventory.StockLimit = inventory.StockLimit;

            await _context.SaveChangesAsync();

            return Ok();

            //var newInventory = new Inventory
            //{
            //    // Stays the same as previous thingies
            //    InventoryID = editInventory.InventoryID,
            //    VarietalID = editInventory.VarietalID,
            //    WineID = editInventory.WineID,
            //    WineTypeID = editInventory.WineTypeID,

            //    // Updated values
            //    QuantityOnHand = inventory.QuantityOnHand,
            //    StockLimit = inventory.StockLimit,
            //};

            //_context.Inventories.Update(newInventory);

            //var result = await _context.SaveChangesAsync();

            //if (result > 0)
            //{
            //    return Ok();
            //}
            //else
            //{
            //    return BadRequest();
            //}
        }

        // POST: api/Inventory
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Inventory>> PostInventory(Inventory inventory)
        {
            var wine = _context.Wines.FirstOrDefault(w => w.WineID == inventory.WineID);

            if(wine == null)
            {
                return NoContent();
            }

            var addInventory = new Inventory
            {
                StockLimit = inventory.StockLimit,
                QuantityOnHand = inventory.QuantityOnHand,
                WineID = wine.WineID,
                WinePrice = (decimal)wine.Price,
                VarietalID = inventory.VarietalID,
                WineTypeID = inventory.WineTypeID,
            };

            _context.Inventories.Add(addInventory);

            var result = await _context.SaveChangesAsync();

           if(result > 0)
            {
                return CreatedAtAction("GetInventory", new { id = inventory.InventoryID }, inventory);
            }
            else
            {
                return BadRequest();
            }
        }

        // DELETE: api/Inventory/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            // GET request to retrieve a specific inventory item by its ID from the database.

            if (inventory == null)
            {
                return NotFound();
                // If the inventory item to be deleted is not found, return a 404 Not Found response.
            }

            _context.Inventories.Remove(inventory);
            // Remove the inventory item from the context.

            await _context.SaveChangesAsync();
            // Save the changes to the database.

            return NoContent();
            // Return a 204 No Content response to indicate successful deletion.
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventories.Any(e => e.InventoryID == id);
            // Check if an inventory item with the specified ID exists in the database.
        }
    }
}
