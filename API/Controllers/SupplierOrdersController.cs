﻿using System;
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

        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<SupplierOrder>>> GetSupplierOrders()
        {
            return await _context.SupplierOrders
                                 .Include(so => so.Supplier)
                                 .Include(so => so.SupplierOrderStatus)
                                 .ToListAsync();
        }

        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<SupplierOrder>> GetSupplierOrder(int id)
        {
            var supplierOrder = await _context.SupplierOrders
                                               .Include(so => so.Supplier)
                                               .Include(so => so.SupplierOrderStatus)
                                               .FirstOrDefaultAsync(so => so.SupplierOrderID == id);

            if (supplierOrder == null)
            {
                return NotFound();
            }

            return supplierOrder;
        }

        [HttpPut("{id}/status")]
        [DynamicAuthorize]
        public async Task<IActionResult> PutSupplierOrder(int id, UpdateSupplierOrderStatusDTO statusDTO)
        {
            if (id != statusDTO.SupplierOrderID)
            {
                return BadRequest();
            }

            // Fetch the existing SupplierOrder from the database
            var supplierOrder = await _context.SupplierOrders.Include(s => s.SupplierOrderStatus).FirstOrDefaultAsync(s => s.SupplierOrderID == id);
            var originalOrderPrice = supplierOrder.OrderTotal;
            if (supplierOrder == null)
            {
                return NotFound();
            }

            // Update the supplierOrderStatus based on the UpdateSupplierOrderStatusDTO
            supplierOrder.SupplierOrderStatus.Ordered = statusDTO.Ordered;
            supplierOrder.SupplierOrderStatus.Paid = statusDTO.Paid;
            supplierOrder.SupplierOrderStatus.Received = statusDTO.Received;

            if(statusDTO.Ordered == true && statusDTO.Paid == true && statusDTO.Received == false) 
            {
                supplierOrder.OrderTotal = statusDTO.orderPrice;
            }else if(statusDTO.Ordered == true && statusDTO.Paid == true && statusDTO.Received == true){
                supplierOrder.OrderTotal = originalOrderPrice;
            }
            else
            {
                supplierOrder.OrderTotal = 0;
            }
            // Update the supplierOrderStatusID in case it has changed
            supplierOrder.SupplierOrderStatus.SupplierOrderStatusID = statusDTO.SupplierOrderStatusID;

            try
            {
                _context.SupplierOrders.Update(supplierOrder);
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


        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<SupplierOrder>> PostSupplierOrder(SupplierOrder supplierOrder)
        {
            // Step 1: Add the SupplierOrder
            _context.SupplierOrders.Add(supplierOrder);
            await _context.SaveChangesAsync(); // This will generate SupplierOrderID

            // Step 2: Add the SupplierOrderStatus entry, referencing the generated SupplierOrderID
            var supplierOrderStatus = new SupplierOrderStatus
            {
                SupplierOrderID = supplierOrder.SupplierOrderID,
                Ordered = false, // set to true if the order is successfully created
                Paid = false, // set initial status
                Received = false // set initial status
            };
            _context.SupplierOrderStatuses.Add(supplierOrderStatus);

            // Step 3: Save the SupplierOrderStatus
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSupplierOrder", new { id = supplierOrder.SupplierOrderID }, supplierOrder);
        }


        [HttpDelete("{id}")]
        [DynamicAuthorize]
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
