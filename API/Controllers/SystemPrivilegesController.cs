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
using API.ViewModels;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Superuser")]
    [Route("api/[controller]")]
    [ApiController]
    public class SystemPrivilegesController : ControllerBase
    {
        private readonly MyDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        public SystemPrivilegesController(MyDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roleManager = roleManager;
        }


        // GET: api/SystemPrivileges
        [HttpGet]
        [Route("GetSystemPrivileges")]
        public async Task<ActionResult<IEnumerable<SystemPrivilege>>> GetSystemPrivileges()
        {
            return await _context.SystemPrivileges.ToListAsync();
        }


        // GET: api/SystemPrivileges/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SystemPrivilege>> GetSystemPrivilege(string id)
        {
            var systemPrivilege = await _context.SystemPrivileges.FindAsync(id);

            if (systemPrivilege == null)
            {
                return NotFound();
            }

            return systemPrivilege;
        }

        // PUT: api/SystemPrivileges/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSystemPrivilege(string id, SystemPrivilege systemPrivilege)
        {
            var existingSystemPrivilege = await _context.SystemPrivileges.FindAsync(id);

            if (existingSystemPrivilege == null)
            {
                return NotFound();
            }

            // Get the role from the AspNetRoles table
            var role = await _roleManager.FindByIdAsync(existingSystemPrivilege.Id);
            if (role != null)
            {
                // Update the role name with the new privilege name
                role.Name = systemPrivilege.Name;
                await _roleManager.UpdateAsync(role);
            }

            // Update the properties of the existingSystemPrivilege with the new values
            existingSystemPrivilege.Name = systemPrivilege.Name;
            existingSystemPrivilege.Description = systemPrivilege.Description;
            // Update other properties as needed

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SystemPrivilegeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(existingSystemPrivilege);
        }


        // POST: api/SystemPrivileges
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("AddPrivilege")]
        public async Task<ActionResult<SystemPrivilege>> PostSystemPrivilege(SystemPrivilegeViewModel viewModel)
        {
            // Check if the role name already exists in SystemPrivileges
            if (await _context.SystemPrivileges.AnyAsync(s => s.Name == viewModel.Name))
            {
                return Conflict("Role name already exists in SystemPrivileges.");
            }
            else
            {
                // Check if the role name already exists in AspNetRoles
                if (await _roleManager.RoleExistsAsync(viewModel.Name))
                {
                    return Conflict("Role name already exists in AspNetRoles.");
                }
                else
                {
                    var role = new IdentityRole { Name = viewModel.Name };

                    // Add role to the AspNetRoles table
                    var result = await _roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        // Handle the error if role creation fails
                        return StatusCode(500, "Failed to create role in AspNetRoles table.");
                    }
                    else
                    {
                        var privilege = new SystemPrivilege
                        {
                            Id = role.Id,
                            Name = viewModel.Name,
                            Description = viewModel.Description
                        };

                        _context.SystemPrivileges.Add(privilege);
                        var savedChanges = await _context.SaveChangesAsync();
                        if(savedChanges > 0)
                        {
                            return CreatedAtAction("GetSystemPrivilege", new { id = privilege.Id }, privilege);
                        }
                        else
                        {
                            await _roleManager.DeleteAsync(role);
                            return BadRequest();
                        }
                    }
                }
            }
            

            

            
        }

        // DELETE: api/SystemPrivileges/5
        [HttpDelete("DeleteSystemPrivilege/{id}")]
        public async Task<IActionResult> DeleteSystemPrivilege(string id)
        {
            var systemPrivilege = await _context.SystemPrivileges.FindAsync(id);
            if (systemPrivilege == null)
            {
                return NotFound();
            }

            // Get the role from the AspNetRoles table
            var role = await _roleManager.FindByIdAsync(systemPrivilege.Id);
            if (role != null)
            {
                // Remove the role from the AspNetRoles table
                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    // Handle the error if role deletion fails
                    return StatusCode(500, "Failed to delete role from AspNetRoles table.");
                }
            }

            _context.SystemPrivileges.Remove(systemPrivilege);
            await _context.SaveChangesAsync();

            return Ok(systemPrivilege);
        }


        private bool SystemPrivilegeExists(string id)
        {
            return _context.SystemPrivileges.Any(e => e.Id.Equals(id));
        }
    }
    // WRITTEN AND SIGNED BY DIHAN DE BOD - TIMESTAMP 11:42 15/05/2023 
}
