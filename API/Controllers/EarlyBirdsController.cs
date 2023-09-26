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
    public class EarlyBirdsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public EarlyBirdsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/EarlyBirds
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<EarlyBird>>> GetAllEarlyBirds()
        {
            return await _context.EarlyBird.ToListAsync();
        }

        // GET: api/EarlyBirds/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<EarlyBird>> GetSingleEarlyBirdEntry(int id)
        {
            var earlyBird = await _context.EarlyBird.FindAsync(id);

            if (earlyBird == null)
            {
                return NotFound();
            }

            return earlyBird;
        }

        // PUT: api/EarlyBirds/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> UpdateEarlyBird(int id, EarlyBird earlyBird)
        {
            if (id != earlyBird.EarlyBirdID)
            {
                return BadRequest();
            }

            _context.Entry(earlyBird).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EarlyBirdExists(id))
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

        // POST: api/EarlyBirds
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<EarlyBird>> AddEarlyBird(EarlyBird earlyBird)
        {
            _context.EarlyBird.Add(earlyBird);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSingleEarlyBirdEntry", new { id = earlyBird.EarlyBirdID }, earlyBird);
        }

        // DELETE: api/EarlyBirds/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteEarlyBird(int id)
        {
            var earlyBird = await _context.EarlyBird.FindAsync(id);
            if (earlyBird == null)
            {
                return NotFound();
            }

            _context.EarlyBird.Remove(earlyBird);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EarlyBirdExists(int id)
        {
            return _context.EarlyBird.Any(e => e.EarlyBirdID == id);
        }
    }
}
