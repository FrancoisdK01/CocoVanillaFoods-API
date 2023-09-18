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
    public class FAQsController : ControllerBase
    {
        private readonly MyDbContext _context;

        public FAQsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/FAQs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FAQ>>> GetFAQs()
        {
            return await _context.FAQs.ToListAsync();
        }

        // GET: api/FAQs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FAQ>> GetFAQ(int id)
        {
            var fAQ = await _context.FAQs.FindAsync(id);

            if (fAQ == null)
            {
                return NotFound();
            }

            return fAQ;
        }

        // PUT: api/FAQs/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> PutFAQ(int id, FAQ fAQ)
        {
            if (id != fAQ.FAQID)
            {
                return BadRequest();
            }

            _context.Entry(fAQ).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FAQExists(id))
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

        // POST: api/FAQs
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [DynamicAuthorize]
        public async Task<ActionResult<FAQ>> PostFAQ(FAQ fAQ)
        {
            _context.FAQs.Add(fAQ);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFAQ", new { id = fAQ.FAQID }, fAQ);
        }

        // DELETE: api/FAQs/5
        [HttpDelete("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteFAQ(int id)
        {
            var fAQ = await _context.FAQs.FindAsync(id);
            if (fAQ == null)
            {
                return NotFound();
            }

            _context.FAQs.Remove(fAQ);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FAQExists(int id)
        {
            return _context.FAQs.Any(e => e.FAQID == id);
        }
    }
}
