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
    public class WriteOffsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public WriteOffsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/WriteOffs
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<WriteOff>>> GetWriteOffs()
        {
            return await _context.WriteOffs.ToListAsync();
        }     

        

        // POST: api/WriteOffs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<WriteOff>> PostWriteOff(WriteOff writeOff)
        {                  

            var newwriteoff = new WriteOff
            {
                WriteOff_Reason = writeOff.WriteOff_Reason,
                WriteOff_Date = writeOff.WriteOff_Date,
                WineName = writeOff.WineName,
                Quantity = writeOff.Quantity
            };

            var addRecord = await _context.WriteOffs.AddAsync(newwriteoff);
            if (addRecord == null)
            {
                return BadRequest("No data that you want to add in the database");
            }
            await _context.SaveChangesAsync();

            return Ok(newwriteoff);

        }

        // DELETE: api/WriteOffs/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteWriteOff(int id)
        {
            var writeOff = await _context.WriteOffs.FindAsync(id);
            if (writeOff == null)
            {
                return NotFound();
            }

            _context.WriteOffs.Remove(writeOff);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WriteOffExists(int id)
        {
            return _context.WriteOffs.Any(e => e.WriteOffID == id);
        }
    }
}
