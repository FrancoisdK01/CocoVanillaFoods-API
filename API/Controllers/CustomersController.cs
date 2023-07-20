using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;

        public CustomersController(MyDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            return await _context.Customers.ToListAsync();
        }

        // GET: api/Customers/5
        [HttpGet]
        [Route("GetUser/{email}")]
        public async Task<IActionResult> GetCustomer(string email)
        {
            var user = await _context.Customers.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                return NotFound("The user you are searching for doesn't exist");
            }
            else
            {
                return Ok(new { user });
            }
        }

        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(string id, Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            var existingUser = await _userManager.FindByIdAsync(existingCustomer.UserID);

            // Update the properties of the existingSystemPrivilege with the new values
            existingCustomer.Id = existingCustomer.Id;
            existingCustomer.Date_Created = existingCustomer.Date_Created;
            existingCustomer.Date_of_last_update = DateTime.Now;

            existingCustomer.First_Name = customer.First_Name;
            existingCustomer.Last_Name = customer.Last_Name;
            existingCustomer.Email = customer.Email;
            existingCustomer.PhoneNumber = customer.PhoneNumber;
            existingCustomer.ID_Number = customer.ID_Number;
            existingCustomer.Gender = customer.Gender;
            existingCustomer.Title = customer.Title;

            existingUser.UserName = customer.First_Name;
            existingUser.Email = customer.Email;
            existingUser.DisplayName = customer.First_Name;
            //existingEmployee.Id = existingEmployee.Id;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(existingCustomer);
        }
        //// POST: api/Customers
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<Customer>> PostCustomer(Customer customer)
        //{
        //    _context.Customers.Add(customer);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction("GetCustomer", new { id = customer.
        //    }, customer);
        //}

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            else
            {
                var user = await _userManager.FindByIdAsync(customer.UserID);
                if (user == null)
                {
                    return NotFound();
                }
                else
                {
                    try
                    {
                        _context.Customers.Remove(customer);
                        await _userManager.DeleteAsync(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }
            return Ok("User has been removed from the system");
        }

        private bool CustomerExists(string id)
        {
            return _context.Customers.Any(e => e.Id.Equals(id));
        }
    }
}
