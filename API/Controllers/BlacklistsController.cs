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
    public class BlacklistsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public BlacklistsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Blacklists
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Blacklist>>> GetBlacklists()
        {
            return await _context.Blacklists.ToListAsync();
        }

        // GET: api/Blacklists/5
        [HttpGet("{id}")]
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
        public async Task<ActionResult<Blacklist>> PostBlacklist(Blacklist blacklist)
        {
            _context.Blacklists.Add(blacklist);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBlacklist", new { id = blacklist.BlacklistID }, blacklist);
        }

        // DELETE: api/Blacklists/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlacklist(int id)
        {
            var blacklist = await _context.Blacklists.FindAsync(id);
            if (blacklist == null)
            {
                return NotFound();
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
        public async Task<ActionResult<bool>> CheckBlacklist(string email)
        {
            var blacklist = await _context.Blacklists
                .Where(b => b.Email == email)
                .FirstOrDefaultAsync();

            return blacklist != null;
        }
    }
}
