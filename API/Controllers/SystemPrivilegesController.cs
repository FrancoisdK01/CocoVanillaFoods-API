﻿using System;
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
        [DynamicAuthorize]
        public async Task<ActionResult<IEnumerable<SystemPrivilege>>> GetAllSystemPrivileges()
        {
            return await _context.SystemPrivileges.ToListAsync();
        }


        // GET: api/SystemPrivileges/5
        [HttpGet("{id}")]
        [DynamicAuthorize]
        public async Task<ActionResult<SystemPrivilege>> GetSingleSystemPrivilegeEntry(string id)
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
        [DynamicAuthorize]
        public async Task<IActionResult> UpdateSystemPrivilege(string id, SystemPrivilegeViewModel systemPrivilege)
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
            var existingMethodMappings = _context.MethodPrivilegeMappings
                                        .Where(m => m.SystemPrivilegeId == id);
            _context.MethodPrivilegeMappings.RemoveRange(existingMethodMappings);

            // Add the new method mappings from the viewModel
            foreach (var mapping in systemPrivilege.ControllerMethods)
            {
                // Optionally, verify if controller and methods exist
                if (!DoesControllerHaveMethods(mapping.ControllerName, mapping.MethodNames))
                {
                    // Handle the error (maybe return a bad request)
                    return BadRequest($"Invalid methods for controller {mapping.ControllerName}.");
                }

                foreach (var methodName in mapping.MethodNames)
                {
                    var methodPrivilege = new MethodPrivilegeMapping
                    {
                        ControllerName = mapping.ControllerName,
                        MethodName = methodName,
                        SystemPrivilegeId = existingSystemPrivilege.Id
                    };

                    _context.MethodPrivilegeMappings.Add(methodPrivilege);
                }
            }
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
        [DynamicAuthorize]
        public async Task<ActionResult<SystemPrivilege>> AddSystemPrivilege(SystemPrivilegeViewModel viewModel)
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
                        if (savedChanges > 0)
                        {
                            foreach (var mapping in viewModel.ControllerMethods)
                            {
                                // Optionally, verify if controller and methods exist
                                if (!DoesControllerHaveMethods(mapping.ControllerName, mapping.MethodNames))
                                {
                                    // Handle the error (maybe return a bad request)
                                    return BadRequest($"Invalid methods for controller {mapping.ControllerName}.");
                                }

                                foreach (var methodName in mapping.MethodNames)
                                {
                                    var methodPrivilege = new MethodPrivilegeMapping
                                    {
                                        ControllerName = mapping.ControllerName,
                                        MethodName = methodName,
                                        SystemPrivilegeId = privilege.Id
                                    };

                                    _context.MethodPrivilegeMappings.Add(methodPrivilege);
                                }
                            }

                            await _context.SaveChangesAsync();

                            return CreatedAtAction("GetSingleSystemPrivilegeEntry", new { id = privilege.Id }, privilege);
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
        [DynamicAuthorize]
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
                var matchedEntries = _context.MethodPrivilegeMappings
                             .Where(m => m.SystemPrivilegeId == systemPrivilege.Id)
                             .ToList();

                if (matchedEntries.Any())
                {
                    _context.MethodPrivilegeMappings.RemoveRange(matchedEntries);
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

        private bool DoesControllerHaveMethods(string controllerName, List<string> methodNames)
        {
            var controllerType = typeof(Startup).Assembly.GetTypes().FirstOrDefault(t => t.Name == controllerName);
            if (controllerType == null) return false;

            foreach (var methodName in methodNames)
            {
                if (controllerType.GetMethod(methodName) == null) return false;
            }

            return true;
        }

        /////////////////////////// METHODMAPPING

        [HttpGet]
        [Route("MethodMapping")]
        public async Task<ActionResult<IEnumerable<object>>> GetMethodMapping()
        {
            var groupedByController = await _context.MethodPrivilegeMappings
                .GroupBy(m => m.ControllerName)
                .Select(g => new
                {
                    ControllerName = g.Key,
                    methodNames = g.Select(m => m.MethodName).Distinct().ToList()
                })
                .ToListAsync();

            return Ok(groupedByController);
        }

        [HttpGet]
        [Route("MethodPrivilegeMapping")]
        public async Task<ActionResult<IEnumerable<object>>> GetMethodPrivilegeMapping()
        {
            var allMethodsAndPrivilegeIds = await _context.MethodPrivilegeMappings
                .Select(g => new
                {
                    MethodName = g.MethodName,
                    PrivilegeID = g.SystemPrivilegeId
                })
                .ToListAsync();

            return Ok(allMethodsAndPrivilegeIds);
        }
    }
    // WRITTEN AND SIGNED BY DIHAN DE BOD - TIMESTAMP 11:42 15/05/2023 
}
