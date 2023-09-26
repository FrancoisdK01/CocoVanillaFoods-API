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
    public class EventPriceController : ControllerBase
    {
        private readonly MyDbContext _context;

        public EventPriceController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/EventPrices
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<EventPrice>>> GetAllEventPrices()
        {
            return await _context.EventPrices.ToListAsync();
        }

        // GET: api/EventPrices/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<EventPrice>> GetSingleEventPrice(int id)
        {
            var eventPrice = await _context.EventPrices.FindAsync(id);

            if (eventPrice == null)
            {
                return NotFound();
            }

            return eventPrice;
        }

        // PUT: api/EventPrices/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> UpdateEventPrice(int id, EventPrice eventPrice)
        {
            if (id != eventPrice.EventPriceID)
            {
                return BadRequest();
            }

            _context.Entry(eventPrice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventPriceExists(id))
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

        // POST: api/EventPrices
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<EventPrice>> AddEventPrice(EventPrice eventPrice)
        {
            _context.EventPrices.Add(eventPrice);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSingleEventPrice", new { id = eventPrice.EventPriceID }, eventPrice);
        }

        // DELETE: api/EventPrices/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteEventPrice(int id)
        {
            var eventPrice = await _context.EventPrices.FindAsync(id);
            if (eventPrice == null)
            {
                return NotFound();
            }

            _context.EventPrices.Remove(eventPrice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventPriceExists(int id)
        {
            return _context.EventPrices.Any(e => e.EventPriceID == id);
        }
    }
}
