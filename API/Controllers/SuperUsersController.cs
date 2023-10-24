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
using System.IdentityModel.Tokens.Jwt;

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
Welcome to the team {registerModel.FirstName},

We are so happy to have you working for us. Please find your login details below and feel free to update your details once you have settled in with the system.

Email Address: {registerModel.Email}
Password: {generatedPassword}

We can't wait to see you in our offices.

Kind regards,
The Promenade Team
    "
                };

                await _emailService.SendSimpleMessage(evm);
                //_emailService.SendEmail(evm);

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

            string authHeader = HttpContext.Request.Headers["Authorization"];
            string token = authHeader.Replace("Bearer ", "");
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);
            var userEmailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            // Retrieve the superuser based on their email
            var superUser = await _userManager.FindByEmailAsync(userEmailClaim);
            if (superUser == null)
            {
                return BadRequest("Superuser not found");
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
                if (roleToAdd == "Employee" || roleToAdd == "Admin")
                {
                    var customerDetailsForEmployeeTable = await _context.Customers.FirstOrDefaultAsync(x => x.Email == model.UserEmail);
                    var employee = new Employee
                    {
                        First_Name = customerDetailsForEmployeeTable.First_Name,
                        Last_Name = customerDetailsForEmployeeTable.Last_Name,
                        Email = customerDetailsForEmployeeTable.Email,
                        PhoneNumber = customerDetailsForEmployeeTable.PhoneNumber,
                        ID_Number = customerDetailsForEmployeeTable.ID_Number,
                        Hire_Date = DateTime.Now,
                        UserId = user.Id,
                        SuperUserID = superUser.Id,
                        TwoFactorEnabled = true
                    };
                    user.TwoFactorEnabled = true;
                    customerDetailsForEmployeeTable.TwoFactorEnabled = true;
                    _context.Employees.Add(employee);
                }
                if (roleToAdd == "Superuser")
                {
                    var customerDetailsForSuperuserTable = await _context.Customers.FirstOrDefaultAsync(x => x.Email == model.UserEmail);
                    var superuser = new SuperUser
                    {
                        First_Name = customerDetailsForSuperuserTable.First_Name,
                        Last_Name = customerDetailsForSuperuserTable.Last_Name,
                        Email = customerDetailsForSuperuserTable.Email,
                        PhoneNumber = customerDetailsForSuperuserTable.PhoneNumber,
                        ID_Number = customerDetailsForSuperuserTable.ID_Number,
                        Hire_Date = DateTime.Now,
                        UserID = user.Id,
                    };
                    _context.SuperUser.Add(superuser);
                }
            }

            // Remove roles from the user
            foreach (var roleToRemove in rolesToRemove)
            {
                if (roleToRemove == "Superuser")
                {
                    var superusersCount = await _context.SuperUser.CountAsync();
                    if (superusersCount <= 1)
                    {
                        // There's only one superuser left. Return a bad request or another suitable response.
                        return BadRequest(new { error = "There must be at least one Superuser in the system. Cannot remove the last Superuser." });
                    }

                    var superuser = await _context.SuperUser.FirstOrDefaultAsync(s => s.UserID == user.Id);
                    if (superuser != null)
                    {
                        _context.SuperUser.Remove(superuser);
                    }
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, roleToRemove);
                    if (roleToRemove == "Employee" || roleToRemove == "Admin")
                    {
                        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.UserId == user.Id);
                        if (employee != null)
                        {
                            _context.Employees.Remove(employee);
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
            var evm = new EmailViewModel
            {
                To = model.UserEmail,
                Subject = "Account Update Notification from the Promenade",
                Body = $@"
Hello {user.DisplayName},

We hope this message finds you well. We wanted to notify you that your account details at the Promenade have been updated successfully. For security reasons and to ensure the integrity of your account, we require you to log in again to see the changes and continue using our system.

If you did not request this change or believe this is an error, please contact our support immediately.

Thank you for your understanding and continued trust in us.

Kind regards,
The Promenade Team
    "
            };



            try
            {
                await _emailService.SendSimpleMessage(evm);
                //_emailService.SendEmail(evm);
                return Ok();
            }
            catch
            {
                return BadRequest(new { message = "Failed to update roles" });
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
