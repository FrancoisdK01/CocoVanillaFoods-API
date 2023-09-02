using API.Data;
using API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockTakeController : ControllerBase
    {
        private readonly MyDbContext _context;

        public StockTakeController(MyDbContext context)
        {
            _context = context; // Constructor that injects the MyDbContext dependency into the controller.
        }

        // GET: api/GetStockTake
        [HttpGet]
        [Route("GetStockTake")]
        public async Task<ActionResult<IEnumerable<StockTake>>> GetStockTake()
        {
            return await _context.StockTakes.ToListAsync();
            // GET request to retrieve all stocktake items from the database.
        }

        [HttpPost]
        [Route("AddStockTake")]
        public async Task<ActionResult<StockTake>> PostStockTake(StockTake stocktake)
        {
            //var wine = _context.Wines.FirstOrDefault(w => w.WineID == stocktake.WineID);

            //if (wine == null)
            //{
            //    return NoContent();
            //}

            var addStockTake = new StockTake
            {
                stocktakeID = stocktake.stocktakeID,
                wineName = stocktake.wineName,
                QuantityOrdered = stocktake.QuantityOrdered,
                QuantityReceived = stocktake.QuantityReceived,
            };
            _context.StockTakes.Add(addStockTake);

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return CreatedAtAction("GetStockTake", new { id = stocktake.stocktakeID }, stocktake);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
