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
            var existingUserInUserTable = await _context.Users.FindAsync(existingCustomer.UserID);

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
                existingCustomer.TwoFactorEnabled = customer.TwoFactorEnabled;

                existingUser.TwoFactorEnabled = customer.TwoFactorEnabled;
                await _userManager.SetTwoFactorEnabledAsync(existingUser, customer.TwoFactorEnabled);
                existingUserInUserTable.TwoFactorEnabled = customer.TwoFactorEnabled;

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
                        existingCustomer.TwoFactorEnabled = customerDetailsBeforeUpdate.TwoFactorEnabled;

                        existingUser.TwoFactorEnabled = customerDetailsBeforeUpdate.TwoFactorEnabled;
                        await _userManager.SetTwoFactorEnabledAsync(existingUser, customerDetailsBeforeUpdate.TwoFactorEnabled);
                        existingUserInUserTable.TwoFactorEnabled = customerDetailsBeforeUpdate.TwoFactorEnabled;

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
                        var cartDetails = await _context.Carts.Include(c => c.CartItems).ThenInclude(ci => ci.Wine).FirstOrDefaultAsync(c => c.CustomerID == id);
                        var wineOrder = await _context.WineOrders.FirstOrDefaultAsync(x => x.CustomerId == id);
                        if (cartDetails == null && wineOrder == null) {
                            _context.Customers.Remove(customer);
                            _context.Users.Remove(user);
                            await _userManager.DeleteAsync(user);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            cartDetails = new Cart { CustomerID = "" };
                            wineOrder = new WineOrder { CustomerId = "" };
                            _context.Customers.Remove(customer);
                            _context.Users.Remove(user);
                            await _userManager.DeleteAsync(user);
                            await _context.SaveChangesAsync();
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }
            return Ok();
        }

        //////////////////////////Marco se code om die age groups in charts te display ////////////////////////////////////////
        [HttpGet]
        [Route("AgeDistribution")]
        public async Task<ActionResult<Dictionary<string, int>>> GetAgeDistribution()
        {
            var users = await _context.Customers.ToListAsync();

            var ageGroups = new Dictionary<string, int>
    {
        {"18-21", 0}, {"21-30", 0}, {"31-40", 0}, {"41-50", 0}, {"51-60", 0}, {"61-70", 0}, {"71-80", 0}, {"81-90", 0}, {"91-100", 0}
    };

            foreach (var user in users)
            {
                // Extract birth year from ID number
                int birthYear;
                string yy = user.ID_Number.Substring(0, 2);
                string century;

                if (int.Parse(yy) <= DateTime.Now.Year % 100)
                {
                    century = "20";
                }
                else
                {
                    century = "19";
                }

                if (int.TryParse(century + yy, out birthYear))
                {
                    var age = DateTime.Now.Year - birthYear;

                    // Classify the user into an age group
                    string group = "";
                    if (age >= 18 && age <= 21) group = "18-21";
                    else if (age > 21 && age <= 30) group = "21-30";
                    else if (age > 31 && age <= 40) group = "31-40";
                    else if (age > 41 && age <= 50) group = "41-50";
                    else if (age > 51 && age <= 60) group = "51-60";
                    else if (age > 61 && age <= 70) group = "61-70";
                    else if (age > 71 && age <= 80) group = "71-80";
                    else if (age > 81 && age <= 90) group = "81-90";
                    else if (age > 91 && age <= 100) group = "91-100";


                    if (ageGroups.ContainsKey(group))
                        ageGroups[group]++;
                }
            }

            return Ok(ageGroups);
        }

    }
}
