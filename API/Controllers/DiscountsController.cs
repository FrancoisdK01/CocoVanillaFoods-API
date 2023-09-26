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

    //final discountscontroller final
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public DiscountsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Discounts
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<Discount>>> GetAllDiscounts()
        {
            return await _context.Discounts.ToListAsync();
        }

        // GET: api/Discounts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Discount>> GetSingleDiscountEntry(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);

            if (discount == null)
            {
                return NotFound();
            }

            return discount;
        }

        // PUT: api/Discounts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> UpdateDiscount(int id, Discount discount)
        {
            if (id != discount.DiscountID)
            {
                return BadRequest();
            }

            _context.Entry(discount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DiscountExists(id))
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

        // POST: api/Discounts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<Discount>> AddDiscount(Discount discount)
        {
            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSingleDiscountEntry", new { id = discount.DiscountID }, discount);
        }

        // DELETE: api/Discounts/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null)
            {
                return NotFound();
            }

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.DiscountID == id);
        }

        [HttpPost("Validate")]
        [DynamicAuthorize]
        public async Task<ActionResult<Discount>> ValidateDiscountCode([FromBody] DiscountCodeDto discountCodeDto)
        {
            Console.WriteLine("Received discount code: " + discountCodeDto.Code);

            string code = discountCodeDto.Code;

            var discount = await _context.Discounts
                .Where(d => d.DiscountCode == code)
                .FirstOrDefaultAsync();

            if (discount == null)
            {
                return NotFound("Invalid discount code");
            }

            // Removing the discount after it's applied
          
            await _context.SaveChangesAsync();

            return discount;
        }

    }
}
