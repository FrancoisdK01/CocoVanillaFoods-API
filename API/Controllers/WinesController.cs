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
    public class WinesController : ControllerBase
    {
        private readonly MyDbContext _context;

        public WinesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Wines
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Wine>>> GetWines()
        {
            return await _context.Wines.ToListAsync();
        }

        // GET: api/Wines/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Wine>> GetWine(int id)
        {
            var wine = await _context.Wines.FindAsync(id);

            if (wine == null)
            {
                return NotFound();
            }

            return wine;
        }

        // PUT: api/Wines/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutWine(int id, Wine wine)
        {
            if (id != wine.WineID)
            {
                return BadRequest();
            }

            // Fetch the WineType and Varietal entities based on the provided IDs
            wine.WineType = await _context.WineTypes.FindAsync(wine.WineTypeID);
            wine.Varietal = await _context.Varietals.FindAsync(wine.VarietalID);

            _context.Entry(wine).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WineExists(id))
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

        // POST: api/Wines
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Wine>> PostWine(Wine wine)
        {
            // Fetch the WineType and Varietal entities based on the provided IDs
            wine.WineType = await _context.WineTypes.FindAsync(wine.WineTypeID);
            wine.Varietal = await _context.Varietals.FindAsync(wine.VarietalID);

            _context.Wines.Add(wine);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetWine", new { id = wine.WineID }, wine);
        }

        // DELETE: api/Wines/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWine(int id)
        {
            var wine = await _context.Wines.FindAsync(id);
            if (wine == null)
            {
                return NotFound();
            }

            _context.Wines.Remove(wine);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool WineExists(int id)
        {
            return _context.Wines.Any(e => e.WineID == id);
        }
    }

}
