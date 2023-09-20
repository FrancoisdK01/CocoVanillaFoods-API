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
    public class WineTypesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public WineTypesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/WineTypes
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<WineType>>> GetWineTypes()
        {
            return await _context.WineTypes.Include(vt => vt.Varietals).ToListAsync();
        }

        // GET: api/WineTypes/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<WineType>> GetWineType(int id)
        {
            var wineType = await _context.WineTypes.FindAsync(id);

            if (wineType == null)
            {
                return NotFound();
            }

            return wineType;
        }

        // PUT: api/WineTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> PutWineType(int id, WineType wineType)
        {
            if (id != wineType.WineTypeID)
            {
                return BadRequest();
            }

            _context.Entry(wineType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WineTypeExists(id))
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

        // POST: api/WineTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<WineType>> PostWineType(WineType wineType)
        {
            _context.WineTypes.Add(wineType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWineType", new { id = wineType.WineTypeID }, wineType);
        }

        // DELETE: api/WineTypes/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteWineType(int id)
        {
            var wineType = await _context.WineTypes.FindAsync(id);

            if (wineType == null)
            {
                return NotFound();
            }

            _context.WineTypes.Remove(wineType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WineTypeExists(int id)
        {
            return _context.WineTypes.Any(e => e.WineTypeID == id);
        }
    }
}
