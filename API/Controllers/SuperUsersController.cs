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
    public class SuperUsersController : ControllerBase
    {
        private readonly MyDbContext _context;

        public SuperUsersController(MyDbContext context)
        {
            _context = context;
        }

        // GET: api/SuperUsers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SuperUser>>> GetSuperUser()
        {
            return await _context.SuperUser.ToListAsync();
        }

        // GET: api/SuperUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SuperUser>> GetSuperUser(string id)
        {
            var superUser = await _context.SuperUser.FindAsync(id);

            if (superUser == null)
            {
                return NotFound();
            }

            return superUser;
        }

        // PUT: api/SuperUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSuperUser(string id, SuperUser superUser)
        {
            if (id != superUser.Id)
            {
                return BadRequest();
            }

            _context.Entry(superUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SuperUserExists(id))
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

        // POST: api/SuperUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SuperUser>> PostSuperUser(SuperUser superUser)
        {
            _context.SuperUser.Add(superUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSuperUser", new { id = superUser.Id }, superUser);
        }

        // DELETE: api/SuperUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSuperUser(string id)
        {
            var superUser = await _context.SuperUser.FindAsync(id);
            if (superUser == null)
            {
                return NotFound();
            }

            _context.SuperUser.Remove(superUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SuperUserExists(string id)
        {
            return _context.SuperUser.Any(e => e.Id == id);
        }
    }
}
