using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierOrdersController : ControllerBase
    {
        private readonly MyDbContext _context;

        public SupplierOrdersController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/SupplierOrders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierOrder>>> GetSupplierOrders()
        {
            return await _context.SupplierOrders.Include(so => so.Supplier).ToListAsync();
        }

        // GET: api/SupplierOrders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierOrder>> GetSupplierOrder(int id)
        {
            var supplierOrder = await _context.SupplierOrders.Include(so => so.Supplier).FirstOrDefaultAsync(so => so.SupplierOrderID == id);

            if (supplierOrder == null)
            {
                return NotFound();
            }

            return supplierOrder;
        }

        // PUT: api/SupplierOrders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSupplierOrder(int id, SupplierOrder supplierOrder)
        {
            if (id != supplierOrder.SupplierOrderID)
            {
                return BadRequest();
            }

            _context.Entry(supplierOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierOrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SupplierOrders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // POST: api/SupplierOrders
        [HttpPost]
        public async Task<ActionResult<SupplierOrder>> PostSupplierOrder([Bind("SupplierOrderID,Quantity_Ordered,DateOrdered,SupplierID,Ordered,Paid,Received")] SupplierOrder supplierOrder)
        {
            _context.SupplierOrders.Add(supplierOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSupplierOrder", new { id = supplierOrder.SupplierOrderID }, supplierOrder);
        }


        // DELETE: api/SupplierOrders/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplierOrder(int id)
        {
            var supplierOrder = await _context.SupplierOrders.FindAsync(id);
            if (supplierOrder == null)
            {
                return NotFound();
            }

            _context.SupplierOrders.Remove(supplierOrder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SupplierOrderExists(int id)
        {
            return _context.SupplierOrders.Any(e => e.SupplierOrderID == id);
        }
    }
}
