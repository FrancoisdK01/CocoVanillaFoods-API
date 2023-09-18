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
    public class WriteOff_ReasonController : ControllerBase
    {
        private readonly MyDbContext _context;

        public WriteOff_ReasonController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/WriteOff_Reason
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<WriteOff_Reason>>> GetWriteOffReasons()
        {
            return await _context.WriteOffReasons.ToListAsync();
        }

        // GET: api/WriteOff_Reason/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<WriteOff_Reason>> GetWriteOff_Reason(int id)
        {
            var writeOff_Reason = await _context.WriteOffReasons.FindAsync(id);

            if (writeOff_Reason == null)
            {
                return NotFound();
            }

            return writeOff_Reason;
        }

        // PUT: api/WriteOff_Reason/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> PutWriteOff_Reason(int id, WriteOff_Reason writeOff_Reason)
        {
            if (id != writeOff_Reason.WriteOff_ReasonID)
            {
                return BadRequest();
            }

            _context.Entry(writeOff_Reason).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WriteOff_ReasonExists(id))
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

        // POST: api/WriteOff_Reason
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<WriteOff_Reason>> PostWriteOff_Reason(WriteOff_Reason writeOff_Reason)
        {
            _context.WriteOffReasons.Add(writeOff_Reason);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWriteOff_Reason", new { id = writeOff_Reason.WriteOff_ReasonID }, writeOff_Reason);
        }

        // DELETE: api/WriteOff_Reason/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteWriteOff_Reason(int id)
        {
            var writeOff_Reason = await _context.WriteOffReasons.FindAsync(id);
            if (writeOff_Reason == null)
            {
                return NotFound();
            }

            _context.WriteOffReasons.Remove(writeOff_Reason);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WriteOff_ReasonExists(int id)
        {
            return _context.WriteOffReasons.Any(e => e.WriteOff_ReasonID == id);
        }
    }
}
