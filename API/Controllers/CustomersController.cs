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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Data;

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
        [Route("GetCustomers")]
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
    

        [HttpPut("{id}")]
        [Authorize(Roles = "Customer")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutEmployee(string id, Customer customer)
        {
            var customerDetailsBeforeUpdate = await _context.Customers.FindAsync(id);
            var existingCustomer = await _context.Customers.FindAsync(id);
            var existingUser = await _userManager.FindByIdAsync(existingCustomer.UserID);

            if (existingCustomer == null)
            {
                return NotFound("There is no employee that has the ID you provided");
            }
            else
            {
                existingCustomer.Id = existingCustomer.Id;

                existingCustomer.First_Name = customer.First_Name;
                existingCustomer.Last_Name = customer.Last_Name;
                existingCustomer.Email = customer.Email;
                existingCustomer.PhoneNumber = customer.PhoneNumber;
                existingCustomer.ID_Number = customer.ID_Number;
                existingCustomer.Title = customer.Title;
                existingCustomer.Gender = customer.Gender;

                var savedcustomerChanges = await _context.SaveChangesAsync();
                if (savedcustomerChanges > 0)
                {
                    if (existingUser == null)
                    {
                        // Revert Customer details to their original state
                        existingCustomer.First_Name = customerDetailsBeforeUpdate.First_Name;
                        existingCustomer.Last_Name = customerDetailsBeforeUpdate.Last_Name;
                        existingCustomer.Email = customerDetailsBeforeUpdate.Email;
                        existingCustomer.PhoneNumber = customerDetailsBeforeUpdate.PhoneNumber;
                        existingCustomer.ID_Number = customerDetailsBeforeUpdate.ID_Number;
                        existingCustomer.Title = customerDetailsBeforeUpdate.Title;
                        existingCustomer.Gender = customerDetailsBeforeUpdate.Gender;

                        await _context.SaveChangesAsync();

                        return NotFound("The user account linked to the Customer account doesn't exist");
                    }
                    else
                    {
                        existingUser.UserName = customer.First_Name;
                        existingUser.Email = customer.Email;
                        existingUser.DisplayName = customer.First_Name;

                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            return BadRequest(ex.Message);
                        }
                    }
                }
            }
            return Ok(existingCustomer);
        }

        // DELETE: api/Customers/5
        [HttpDelete("DeleteCustomer/{id}")]
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
                        _context.Users.Remove(user);
                        await _userManager.DeleteAsync(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }
            return Ok();
        }
    }
}
