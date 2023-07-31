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
            if (id != inventory.InventoryID)
            {
                return BadRequest();
                // If the ID provided in the URL does not match the ID in the request body, return a 400 Bad Request response.
            }

            _context.Entry(inventory).State = EntityState.Modified;
            // Update the state of the inventory object to Modified to indicate it has been changed.

            try
            {
                await _context.SaveChangesAsync();
                // Save the changes to the database.
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(id))
                {
                    return NotFound();
                    // If the inventory item to be updated is not found, return a 404 Not Found response.
                }
                else
                {
                    throw;
                    // If there's a concurrency issue while updating, rethrow the exception.
                }
            }

            return NoContent();
            // If the update is successful, return a 204 No Content response.
        }

        // POST: api/Inventory
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Inventory>> PostInventory(Inventory inventory)
        {
            _context.Inventories.Add(inventory);
            // Add the new inventory item to the context.

            await _context.SaveChangesAsync();
            // Save the changes to the database.

            return CreatedAtAction("GetInventory", new { id = inventory.InventoryID }, inventory);
            // Return a 201 Created response with the newly created inventory item and its location.
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
