using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using API.ViewModels;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlacklistsController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly IEmailService _emailService;

        public BlacklistsController(MyDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;   
        }

        // GET: api/Blacklists
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<Blacklist>>> GetBlacklists()
        {
            return await _context.Blacklists.ToListAsync();
        }

        // GET: api/Blacklists/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<Blacklist>> GetBlacklist(int id)
        {
            var blacklist = await _context.Blacklists.FindAsync(id);

            if (blacklist == null)
            {
                return NotFound();
            }

            return blacklist;
        }

        // PUT: api/Blacklists/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> PutBlacklist(int id, Blacklist blacklist)
        {
            if (id != blacklist.BlacklistID)
            {
                return BadRequest();
            }

            _context.Entry(blacklist).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlacklistExists(id))
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

        // POST: api/Blacklists
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<Blacklist>> PostBlacklist(Blacklist blacklist)
        {
            _context.Blacklists.Add(blacklist);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBlacklist", new { id = blacklist.BlacklistID }, blacklist);
        }

        // DELETE: api/Blacklists/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteBlacklist(BlacklistDeleteViewModel deleteViewModel)
        {
            var blacklist = await _context.Blacklists.FindAsync(deleteViewModel.id);
            if (blacklist == null)
            {
                return NotFound();
            }
            var customerName = _context.Customers.FirstOrDefault(x => x.Email == blacklist.Email);

            var email = new EmailViewModel
            {
                To = blacklist.Email,
                Subject = "Your account has been removed from the blacklist.",
                Body = $@"
                    <h1>Hello {customerName.First_Name + " " + customerName.Last_Name},</h1>
                    <p>We are pleased to inform you that your account has been removed from our blacklist. This means you are now eligible to purchase event tickets using your account.</p>
                    <p><strong>Reason for removal:</strong> {deleteViewModel.reason}</p>
                    <p>We apologize for any inconvenience caused during the period your account was blacklisted. We value your patronage and look forward to serving you in the future.</p>
                    <p>If you have any questions or concerns, please do not hesitate to contact our support team.</p>
                    <p>Thank you for your understanding and continued trust in us.</p>
                    <p>Kind regards,</p>
                    <p>[Your Company Name] Team</p>
                "
            };

            try
            {
                _emailService.SendEmail(email);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            _context.Blacklists.Remove(blacklist);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BlacklistExists(int id)
        {
            return _context.Blacklists.Any(e => e.BlacklistID == id);
        }

        //Check if users are in Blacklist
        [HttpGet("check/{email}")]
        [DynamicAuthorize]
        public async Task<ActionResult<bool>> CheckBlacklist(string email)
        {
            var blacklist = await _context.Blacklists
                .Where(b => b.Email == email)
                .FirstOrDefaultAsync();

            return blacklist != null;
        }
    }
}
