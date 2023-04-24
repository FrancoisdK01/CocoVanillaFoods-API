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
    public class SupplierPaymentsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public SupplierPaymentsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/SupplierPayments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SupplierPayment>>> GetSupplierPayments()
        {
            return await _context.SupplierPayments.ToListAsync();
        }

        // GET: api/SupplierPayments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SupplierPayment>> GetSupplierPayment(int id)
        {
            var supplierPayment = await _context.SupplierPayments.FindAsync(id);

            if (supplierPayment == null)
            {
                return NotFound();
            }

            return supplierPayment;
        }

        // PUT: api/SupplierPayments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSupplierPayment(int id, SupplierPayment supplierPayment)
        {
            if (id != supplierPayment.SupplierPaymentID)
            {
                return BadRequest();
            }

            _context.Entry(supplierPayment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SupplierPaymentExists(id))
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

        // POST: api/SupplierPayments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SupplierPayment>> PostSupplierPayment(SupplierPayment supplierPayment)
        {
            _context.SupplierPayments.Add(supplierPayment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSupplierPayment", new { id = supplierPayment.SupplierPaymentID }, supplierPayment);
        }

        // DELETE: api/SupplierPayments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplierPayment(int id)
        {
            var supplierPayment = await _context.SupplierPayments.FindAsync(id);
            if (supplierPayment == null)
            {
                return NotFound();
            }

            _context.SupplierPayments.Remove(supplierPayment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SupplierPaymentExists(int id)
        {
            return _context.SupplierPayments.Any(e => e.SupplierPaymentID == id);
        }
    }
}
