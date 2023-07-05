﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using API.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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

        public EmployeesController(MyDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            var result = await _userManager.CreateAsync(user, registerModel.Password);
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

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EmployeeExists(string id)
        {
            return _context.Employees.Any(e => e.Id.Equals(id));
        }
    }
}
