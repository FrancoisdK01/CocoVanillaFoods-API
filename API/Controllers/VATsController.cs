using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VATsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public VATsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/VATs
        [HttpGet]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<VAT>>> GetAllVATs()
        {
            return await _context.VATs.ToListAsync();
        }

        // GET: api/VATs/Latest
        [HttpGet("Latest")]
        [DynamicAuthorize]
        public async Task<IActionResult> GetSingleVATEntry()
        {
            try
            {
                DateTime currentDate = DateTime.UtcNow.AddHours(2);

                var activeVAT = await _context.VATs
                                    .Where(v => v.Date <= currentDate)
                                    .OrderByDescending(v => v.Date)
                                    .FirstOrDefaultAsync();

                if (activeVAT == null)
                {
                    return NotFound("No VAT record found.");
                }

                // Use a proper logging mechanism here if needed
                // _logger.LogInformation($"Latest VAT: {activeVAT}");

                return Ok(activeVAT.Percentage);
            }
            catch (Exception ex)
            {
                // Log the exception using a proper logging mechanism
                // _logger.LogError(ex, "Error fetching the latest VAT");
                return StatusCode(500, "Internal server error");
            }
        }




        // PUT: api/VATs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> UpdateVAT(int id, VAT vAT)
        {
            if (id != vAT.VATID)
            {
                return BadRequest();
            }

            _context.Entry(vAT).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VATExists(id))
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

        // POST: api/VATs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<VAT>> AddVAT(VAT vAT)
        {
            _context.VATs.Add(vAT);
            await _context.SaveChangesAsync();

            return CreatedAtAction("AddVAT", new { id = vAT.VATID }, vAT);
        }

        // DELETE: api/VATs/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteVAT(int id)
        {
            var vAT = await _context.VATs.FindAsync(id);
            if (vAT == null)
            {
                return NotFound();
            }

            _context.VATs.Remove(vAT);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VATExists(int id)
        {
            return _context.VATs.Any(e => e.VATID == id);
        }
    }
}
