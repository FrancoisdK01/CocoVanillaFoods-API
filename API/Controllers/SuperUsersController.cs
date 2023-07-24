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
using Microsoft.AspNetCore.Identity;
using API.ViewModels;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Superuser")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SuperUsersController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SuperUsersController(MyDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: api/SuperUsers
        [HttpGet]
        [Route("GetSuperusers")]
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
            var existingSuperUser = await _context.SuperUser.FindAsync(id);

            if (existingSuperUser == null)
            {
                return NotFound();
            }

            // Update the properties of the existingSystemPrivilege with the new values
            existingSuperUser.Id = existingSuperUser.Id;
            existingSuperUser.Hire_Date = existingSuperUser.Hire_Date;

            existingSuperUser.First_Name = superUser.First_Name;
            existingSuperUser.Last_Name = superUser.Last_Name;
            existingSuperUser.Email = superUser.Email;
            existingSuperUser.PhoneNumber = superUser.PhoneNumber;
            existingSuperUser.ID_Number = superUser.ID_Number;

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
            return Ok(existingSuperUser);
        }

        // POST: api/SuperUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("AddSuperuser")]
        public async Task<ActionResult<SuperUser>> PostSuperUser(SuperUserRegistrationViewModel viewModel)
        {
            RegisterViewModel registerModel = viewModel.RegisterModel;
            SuperUserViewModel superUserModel = viewModel.SuperUserModel;

            // Create the user account
            var user = new User { UserName = registerModel.DisplayName, Email = registerModel.Email, DisplayName = registerModel.DisplayName };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var result = await _userManager.CreateAsync(user, registerModel.Password);
            if (!result.Succeeded)
            {
                // Handle user account creation failure
                return BadRequest(result.Errors);
            }

            // Create the employee account
            var superuser = new SuperUser
            {
                First_Name = superUserModel.FirstName,
                Last_Name = superUserModel.LastName,
                Email = registerModel.Email,
                PhoneNumber = superUserModel.PhoneNumber,
                ID_Number = superUserModel.IDNumber,
                Hire_Date = DateTime.Now,
                UserID = user.Id,
            };

            _context.SuperUser.Add(superuser);
            await _context.SaveChangesAsync();

            if (_context.Entry(superuser).State == EntityState.Unchanged)
            {
                // Changes have been saved
                // Add roles to the user account
                await _userManager.AddToRoleAsync(user, "Superuser");
                await _userManager.AddToRoleAsync(user, "Customer");
            }
            return CreatedAtAction("GetSuperUser", new { id = superuser.Id }, superuser);
        }

        // DELETE: api/SuperUsers/5
        [HttpDelete("DeleteSuperuser/{id}")]
        public async Task<IActionResult> DeleteSuperUser(string id)
        {
            var superuser = await _context.SuperUser.FindAsync(id);
            if (superuser == null)
            {
                return NotFound();
            }
            else
            {
                var user = await _userManager.FindByIdAsync(superuser.UserID);
                if(user == null)
                {
                    return NotFound();
                }
                else
                {
                    try
                    {
                        _context.SuperUser.Remove(superuser);
                        await _userManager.DeleteAsync(user);
                        await _context.SaveChangesAsync();
                    }catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }
            return Ok("The superuser has been removed from the system");
        }

        private bool SuperUserExists(string id)
        {
            return _context.SuperUser.Any(e => e.Id == id);
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<UserRolesViewModel>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            var userRolesViewModels = new List<UserRolesViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                var userViewModel = new UserRolesViewModel
                {
                    UserEmail = user.Email,
                    Privileges = roles.ToList()
                };

                userRolesViewModels.Add(userViewModel);
            }

            return userRolesViewModels;
        }

        [HttpPost]
        [Route("UpdateUserRoles")]
        public async Task<IActionResult> UpdateUserRoles(UserRolesViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.UserEmail);

            if (user == null)
            {
                return NotFound();
            }

            // Get all roles from the database
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            // Determine roles to be added and removed
            var userRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = model.Privileges.Intersect(allRoles).Except(userRoles);
            var rolesToRemove = userRoles.Except(model.Privileges);

            // Add roles to the user
            foreach (var roleToAdd in rolesToAdd)
            {
                await _userManager.AddToRoleAsync(user, roleToAdd);
            }

            // Remove roles from the user
            foreach (var roleToRemove in rolesToRemove)
            {
                await _userManager.RemoveFromRoleAsync(user, roleToRemove);
            }
            return Ok();
        }

        [HttpGet("GetAllRoles")]
        public async Task<IEnumerable<string>> GetAllRoles()
        {
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return allRoles;
        }
    }
}
