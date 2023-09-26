using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;
        private static Random random = new Random();
        private readonly IEmailService _emailService;
        public EmployeesController(MyDbContext context, UserManager<User> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // GET: api/Employees
        [HttpGet]
        [Route("GetEmployees")]
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return await _context.Employees.ToListAsync();
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<Employee>> GetEmployee(string id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound();
            }

            return employee;
        }

        // PUT: api/Employees/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> PutEmployee(string id, Employee employee)
        {
            var employeeDetailsBeforeUpdate = await _context.Employees.FindAsync(id);
            var existingEmployee = await _context.Employees.FindAsync(id);
            var existingUser = await _userManager.FindByIdAsync(existingEmployee.UserId);

            if (existingEmployee == null)
            {
                return NotFound("There is no employee that has the ID you provided");
            }
            else
            {
                existingEmployee.Id = existingEmployee.Id;
                existingEmployee.Hire_Date = employee.Hire_Date;

                existingEmployee.First_Name = employee.First_Name;
                existingEmployee.Last_Name = employee.Last_Name;
                existingEmployee.Email = employee.Email;
                existingEmployee.PhoneNumber = employee.PhoneNumber;
                existingEmployee.ID_Number = employee.ID_Number;
                existingEmployee.SuperUserID = existingEmployee.SuperUserID;

                var savedEmployeeChanges = await _context.SaveChangesAsync();
                if (savedEmployeeChanges > 0)
                {
                    if (existingUser == null)
                    {
                        // Revert employee details to their original state
                        existingEmployee.Hire_Date = employeeDetailsBeforeUpdate.Hire_Date;
                        existingEmployee.First_Name = employeeDetailsBeforeUpdate.First_Name;
                        existingEmployee.Last_Name = employeeDetailsBeforeUpdate.Last_Name;
                        existingEmployee.Email = employeeDetailsBeforeUpdate.Email;
                        existingEmployee.PhoneNumber = employeeDetailsBeforeUpdate.PhoneNumber;
                        existingEmployee.ID_Number = employeeDetailsBeforeUpdate.ID_Number;

                        await _context.SaveChangesAsync();

                        return NotFound("The user account linked to the employee account doesn't exist");
                    }
                    else
                    {
                        existingUser.UserName = employee.First_Name;
                        existingUser.Email = employee.Email;
                        existingUser.DisplayName = employee.First_Name;

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
            return Ok(existingEmployee);
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("AddEmployee")]
        [DynamicAuthorize]
        public async Task<ActionResult<Employee>> PostEmployee(EmployeeRegistrationViewModel viewModel)
        {
            RegisterViewModel registerModel = viewModel.RegisterModel;
            EmployeeViewModel employeeModel = viewModel.EmployeeModel;

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
            else
            {
                var superuserID = superUser.Id;

                var user = new User { UserName = registerModel.DisplayName, Email = registerModel.Email, DisplayName = registerModel.DisplayName, TwoFactorEnabled = true };
                var generatedPassword = GeneratePassword();
                var result = await _userManager.CreateAsync(user, generatedPassword);

                if (!result.Succeeded)
                {
                    await _userManager.DeleteAsync(user);

                    return BadRequest(result.Errors);
                }
                else
                {
                    var employee = new Employee
                    {
                        First_Name = employeeModel.FirstName,
                        Last_Name = employeeModel.LastName,
                        Email = registerModel.Email,
                        PhoneNumber = employeeModel.PhoneNumber,
                        ID_Number = employeeModel.IDNumber,
                        Hire_Date = DateTime.Now,
                        UserId = user.Id,
                        SuperUserID = superuserID
                    };

                    _context.Employees.Add(employee);
                    var employeeSavedChanges = await _context.SaveChangesAsync();

                    if (employeeSavedChanges > 0)
                    {
                        await _userManager.AddToRoleAsync(user, "Employee");
                        await _userManager.AddToRoleAsync(user, "Customer");

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

                        return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee);
                    }
                    else
                    {
                        return BadRequest("Failed to add employee account to the database");
                    }
                }
            }
        }

        // DELETE: api/Employees/5
        [HttpDelete("DeleteEmployee/{id}")]
        [DynamicAuthorize]
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            else
            {
                var user = await _userManager.FindByIdAsync(employee.UserId);
                if (user == null)
                {
                    return NotFound();
                }
                else
                {
                    try
                    {
                        _context.Employees.Remove(employee);
                        await _userManager.DeleteAsync(user);
                        await _context.SaveChangesAsync();
                    }
                    catch(Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
            }
            return Ok();
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.Id.Equals(id));
        }

        [DynamicAuthorize]
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
