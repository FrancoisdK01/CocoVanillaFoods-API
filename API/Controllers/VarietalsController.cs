﻿using System;
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
    public class VarietalsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public VarietalsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/Varietals
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<Varietal>>> GetAllVarietals()
        {
            return await _context.Varietals.Include(wt => wt.WineType).ToListAsync();
        }

        // GET: api/Varietals/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<Varietal>> GetSingleVarietalEntry(int id)
        {
            var varietal = await _context.Varietals.FindAsync(id);

            if (varietal == null)
            {
                return NotFound();
            }

            return varietal;
        }

        // PUT: api/Varietals/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> UpdateVarietal(int id, Varietal varietal)
        {
            if (id != varietal.VarietalID)
            {
                return BadRequest();
            }

            _context.Entry(varietal).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VarietalExists(id))
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

     
        // POST: api/Varietals
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<Varietal>> AddVarietal(Varietal varietal)
        {
            var existingWineType = await _context.WineTypes.FindAsync(varietal.WineTypeID);

            if (existingWineType == null)
            {
                return BadRequest("WineType with provided WineTypeID does not exist.");
            }

            // Explicitly set the WineType to null so Entity Framework doesn't try to create a new one.
            varietal.WineType = null;

            _context.Varietals.Add(varietal);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSingleVarietalEntry", new { id = varietal.VarietalID }, varietal);
        }



        // DELETE: api/Varietals/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteVarietal(int id)
        {
            var varietal = await _context.Varietals.FindAsync(id);
            if (varietal == null)
            {
                return NotFound();
            }

            _context.Varietals.Remove(varietal);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VarietalExists(int id)
        {
            return _context.Varietals.Any(e => e.VarietalID == id);
        }
    }
}
