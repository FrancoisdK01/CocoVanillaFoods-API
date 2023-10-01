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
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<StockTake>>> GetAllStockTakes()
        {
            // Order stocktake items by DateDone before returning them
            return await _context.StockTakes
                                 .OrderByDescending(stockTake => stockTake.DateDone)
                                 .ToListAsync();
        }


        [HttpPost]
        [Route("AddStockTake")]
        [DynamicAuthorize]
        public async Task<ActionResult<StockTake>> AddStockTake(StockTake stocktake)
        {
            var addStockTake = new StockTake
            {
                stocktakeID = stocktake.stocktakeID,
                DateDone = DateTime.Now,
                QuantityOrdered = stocktake.QuantityOrdered,
                QuantityReceived = stocktake.QuantityReceived,
                SupplierOrderID = stocktake.SupplierOrderID,
                Added = false
            };

            var supplierOrder = _context.SupplierOrders.FirstOrDefault(sO => sO.SupplierOrderID == stocktake.SupplierOrderID);
            if (supplierOrder == null)
            {
                return BadRequest();
            }

            var inventoryToUpdate = _context.Inventories.FirstOrDefault(i => i.InventoryID == supplierOrder.InventoryID);
            if (inventoryToUpdate == null)
            {
                return BadRequest();
            }

            var checkReceivedEqualsOrdered = stocktake.QuantityOrdered - stocktake.QuantityReceived;
            if (checkReceivedEqualsOrdered > 0)
            {
                var backOrder = new SupplierOrder
                {
                    SupplierOrderRefNum = "B-" + supplierOrder.SupplierOrderRefNum,
                    Quantity_Ordered = checkReceivedEqualsOrdered,
                    DateOrdered = DateTime.Now,
                    OrderTotal = 0,
                    SupplierID = supplierOrder.SupplierID,
                    InventoryID = supplierOrder.InventoryID,
                    isBackOrder = true
                };

                var supplierOrderStatus = new SupplierOrderStatus
                {
                    Ordered = true,
                    Paid = true,
                    Received = false
                };

                backOrder.SupplierOrderStatus = supplierOrderStatus; // Set the navigation property
                _context.SupplierOrders.Add(backOrder);
                // No need to add supplierOrderStatus separately, it will be added along with backOrder
            }

            _context.StockTakes.Add(addStockTake);
            inventoryToUpdate.QuantityOnHand += stocktake.QuantityReceived;

            try
            {
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    return CreatedAtAction("GetAllStockTakes", new { id = stocktake.stocktakeID }, stocktake);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }



        [HttpPut("UpdateStockTake/{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<StockTake>> UpdateStockTake(StockTake stocktake)
        {

            var putStockTake = new StockTake
            {
                stocktakeID = stocktake.stocktakeID,
                DateDone = stocktake.DateDone,
                QuantityOrdered = stocktake.QuantityOrdered,
                QuantityReceived = stocktake.QuantityReceived,
                Added = stocktake.Added
            };
            _context.StockTakes.Update(putStockTake);

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return CreatedAtAction("GetAllStockTakes", new { id = stocktake.stocktakeID }, stocktake);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
