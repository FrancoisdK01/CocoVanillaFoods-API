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
    public class SuperUsersController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;
        private static Random random = new Random();
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public SuperUsersController(MyDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        // GET: api/SuperUsers
        [HttpGet]
        [Route("GetSuperusers")]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<SuperUser>>> GetAllSuperUsers()
        {
            return await _context.SuperUser.ToListAsync();
        }

        // GET: api/SuperUsers/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<SuperUser>> GetSingleSuperUserEntry(string id)
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
        [HttpPut("UpdateSuperUser/{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> UpdateSuperUserAccount(string id, SuperUser superUser)
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
        [DynamicAuthorize]
        public async Task<ActionResult<SuperUser>> AddSuperUserAccount(SuperUserRegistrationViewModel viewModel)
        {
            RegisterViewModel registerModel = viewModel.RegisterModel;
            SuperUserViewModel superUserModel = viewModel.SuperUserModel;

            // Create the user account
            var user = new User { UserName = registerModel.DisplayName, Email = registerModel.Email, DisplayName = registerModel.DisplayName };
            var generatedPassword = GeneratePassword();
            var result = await _userManager.CreateAsync(user, generatedPassword);
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

            var customer = new Customer
            {
                First_Name = superUserModel.FirstName,
                Last_Name = superUserModel.LastName,
                Email = registerModel.Email,
                PhoneNumber = superUserModel.PhoneNumber,
                ID_Number = superUserModel.IDNumber,
                Date_Created = DateTime.Now,
                Date_of_last_update = DateTime.Now,
                UserID = user.Id,
                Gender = registerModel.Gender,
                Title = registerModel.Title,
            };

            _context.SuperUser.Add(superuser);
            _context.Customers.Add(customer);
            var superUserSavedChanges = await _context.SaveChangesAsync();

            if(superUserSavedChanges > 0)
            {
                if (_context.Entry(superuser).State == EntityState.Unchanged)
                {
                    // Changes have been saved
                    // Add roles to the user account
                    await _userManager.AddToRoleAsync(user, "Superuser");
                    await _userManager.AddToRoleAsync(user, "Customer");
                }

                var evm = new EmailViewModel
                {
                    To = registerModel.Email,
                    Subject = "Welcome to the Promenade",
                    Body = $@"
                                        <h1>Welcome to the team {registerModel.FirstName}</h1>
                                        <p>We are so happy to have you working for us.</p>
                                        <p>Please find your login details below and feel free to update your details once you have settled in with the system.</p>
                                        <ul>
                                            <li>Email Address: {registerModel.Email}</li>
                                            <li>Password: {generatedPassword}</li>
                                        </ul>
                                        <p>We can't wait to see you in our offices.</p>
                                        <p>Kind regards,</p>
                                        <p>The Promenade Team</p>
                                        "
                };

                _emailService.SendEmail(evm);

                return CreatedAtAction("GetSingleSuperUserEntry", new { id = superuser.Id }, superuser);
            }
            else
            {
                return BadRequest("Failed to add superuser");
            }

           
        }

        // DELETE: api/SuperUsers/5
        [HttpDelete("DeleteSuperuser/{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteSuperUserAccount(string id)
        {
            var superuser = await _context.SuperUser.FindAsync(id);
            if (superuser == null)
            {
                return NotFound();
            }

            if (superuser.Email.Equals("marinda.bloem@promenade.com"))
            {
                return BadRequest();
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
            return Ok();
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

        [HttpPut]
        [Route("UpdateUserRoles")]
        [DynamicAuthorize]
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

            var evm = new EmailViewModel
            {
                To = model.UserEmail,
                Subject = "Account Update Notification from the Promenade",
                Body = $@"
                        <h1>Hello {user.DisplayName},</h1>
                        <p>We hope this message finds you well. We wanted to notify you that your account details at the Promenade have been updated successfully.</p>
                        <p>For security reasons and to ensure the integrity of your account, we require you to log in again to see the changes and continue using our system.</p>
                        
                        <p>If you did not request this change or believe this is an error, please contact our support immediately.</p>
                        <p>Thank you for your understanding and continued trust in us.</p>
                        <p>Kind regards,</p>
                        <p>The Promenade Team</p>
                    "
            };


            try
            {
                _emailService.SendEmail(evm);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
            
        }

        [HttpGet("GetAllRoles")]
        [DynamicAuthorize]
        public async Task<IEnumerable<string>> GetAllRoles()
        {
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return allRoles;
        }


        public static string GeneratePassword()
        {
            string uppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string specialCharacters = "!@#$&*";
            string digits = "0123456789";
            string lowercaseLetters = "abcdefghijklmnopqrstuvwxyz";

            char[] password = new char[12];

            // Select one character from each requirement
            password[0] = GetRandomCharacter(uppercaseLetters);
            password[1] = GetRandomCharacter(specialCharacters);
            password[2] = GetRandomCharacter(digits);
            password[3] = GetRandomCharacter(lowercaseLetters);

            // Fill the remaining characters
            for (int i = 4; i < 12; i++)
            {
                string allCharacters = uppercaseLetters + specialCharacters + digits + lowercaseLetters;
                password[i] = GetRandomCharacter(allCharacters);
            }

            // Shuffle the password characters
            for (int i = 0; i < 12; i++)
            {
                int randomIndex = random.Next(i, 12);
                char temp = password[randomIndex];
                password[randomIndex] = password[i];
                password[i] = temp;
            }

            return new string(password);
        }

        private static char GetRandomCharacter(string characterSet)
        {
            int randomIndex = random.Next(0, characterSet.Length);
            return characterSet[randomIndex];
        }
    }
}
