using API.Data;
using API.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketPurchasesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public TicketPurchasesController(MyDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<TicketPurchase>> PostTicketPurchase(TicketPurchase ticketPurchase)
        {
            _context.TicketPurchases.Add(ticketPurchase);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicketPurchase), new { id = ticketPurchase.Id }, ticketPurchase);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketPurchase>> GetTicketPurchase(int id)
        {
            var ticketPurchase = await _context.TicketPurchases.FindAsync(id);

            if (ticketPurchase == null)
            {
                return NotFound();
            }

            return ticketPurchase;
        }

        // New method to get all purchases for a user
        [HttpGet("User/{userEmail}")]
        public async Task<ActionResult<IEnumerable<TicketPurchase>>> GetPurchasesForUser(string userEmail)
        {
            var ticketPurchases = await _context.TicketPurchases
                .Where(tp => tp.UserEmail == userEmail)
                .ToListAsync();

            if (ticketPurchases == null || ticketPurchases.Count == 0)
            {
                return NotFound();
            }

            return ticketPurchases;
        }
        [HttpDelete("CheckAndDelete/{id}")]
        public async Task<IActionResult> CheckAndDeletePurchasedTicket(int id)
        {
            var ticketPurchase = await _context.TicketPurchases.FindAsync(id);

            if (ticketPurchase == null)
            {
                return NotFound();
            }

            if (ticketPurchase.EventDeleted)
            {
                _context.TicketPurchases.Remove(ticketPurchase);
                await _context.SaveChangesAsync();

                return NoContent();
            }

            return BadRequest(new { message = "The ticket purchase cannot be deleted as the event has not been deleted." });
        }



    }
}
