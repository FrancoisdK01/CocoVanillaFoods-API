using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Org.BouncyCastle.Asn1.Cmp;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Superuser")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            return await _context.Employees.ToListAsync();
        }

        // GET: api/Employees/5
        [HttpGet("{id}")]
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
        [Authorize(Roles = "Employee,Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutEmployee(string id, Employee employee)
        {
            var existingEmployee = await _context.Employees.FindAsync(id);

            if (existingEmployee == null)
            {
                return NotFound();
            }

            var existingUser = await _userManager.FindByIdAsync(existingEmployee.UserId);

            // Update the properties of the existingSystemPrivilege with the new values
            existingEmployee.Id = existingEmployee.Id;
            existingEmployee.Hire_Date = employee.Hire_Date;

            existingEmployee.First_Name = employee.First_Name;
            existingEmployee.Last_Name = employee.Last_Name;
            existingEmployee.Email = employee.Email;
            existingEmployee.PhoneNumber = employee.PhoneNumber;
            existingEmployee.ID_Number = employee.ID_Number;
            existingEmployee.SuperUserID = existingEmployee.SuperUserID;

            existingUser.UserName = employee.First_Name;
            existingUser.Email = employee.Email;
            existingUser.DisplayName = employee.First_Name;
            //existingEmployee.Id = existingEmployee.Id;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(existingEmployee);
        }

        // POST: api/Employees
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Employee>> PostEmployee(EmployeeRegistrationViewModel viewModel)
        {
            RegisterViewModel registerModel = viewModel.RegisterModel;
            EmployeeViewModel employeeModel = viewModel.EmployeeModel;

            // Retrieve the superuser ID from the logged-in user
            var superuser = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Retrieve the superuser based on their email
            var superUser = await _userManager.FindByEmailAsync(superuser);
            if (superUser == null)
            {
                // Handle superuser not found
                return BadRequest("Superuser not found");
            }

            // Get the ID of the superuser
            var superuserID = superUser.Id;


            // Create the user account
            var user = new User { UserName = registerModel.DisplayName, Email = registerModel.Email, DisplayName = registerModel.DisplayName };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var generatedPassword = GeneratePassword();
            var result = await _userManager.CreateAsync(user, generatedPassword); // Change to GeneratePassword();
            if (!result.Succeeded)
            {
                // Handle user account creation failure
                return BadRequest(result.Errors);
            }

            // Create the employee account
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
            await _context.SaveChangesAsync();

            if (_context.Entry(employee).State == EntityState.Unchanged)
            {
                // Changes have been saved
                // Add roles to the user account
                await _userManager.AddToRoleAsync(user, "Employee");
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


            return CreatedAtAction("GetEmployee", new { id = employee.Id }, employee);
        }

        // DELETE: api/Employees/5
        [HttpDelete("{id}")]
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

            
           

            return Ok("User has been removed from the system");
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.Id.Equals(id));
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
