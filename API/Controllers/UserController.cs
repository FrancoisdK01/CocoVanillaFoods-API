﻿using API.Data;
using API.Model;
using API.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly MyDbContext _context;
        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration, MyDbContext context) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = configuration;
            _context = context;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<UserViewModel>> Login(LoginViewModel lvm)
        {

            var user = await _userManager.FindByEmailAsync(lvm.email);

            if (user != null && await _userManager.CheckPasswordAsync(user, lvm.password))
            {
                try
                {
                    return GenerateJWTToken(user);
                }
                catch (Exception)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error. Please contact support.");
                }
            }
            else
            {
                return NotFound("Does not exist");
            }
        }


        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<UserViewModel>> Register(RegisterViewModel rvm)
        {
            var user = await _userManager.FindByEmailAsync(rvm.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = rvm.Email,
                    Email = rvm.Email,
                    DisplayName = rvm.DisplayName
                };

                var result = await _userManager.CreateAsync(user, rvm.Password);

                if (result.Succeeded)
                {
                    // Add the default role to the user
                    await _userManager.AddToRoleAsync(user, "Customer");
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error. Please contact support.");
                }
            }
            else
            {
                return Forbid("Account already exists.");
            }

            return Ok(user);
        }

        [HttpGet]
        private ActionResult GenerateJWTToken(User user)
        {
            var roles = _userManager.GetRolesAsync(user).Result;

            // Create JWT Token
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            // Add the role claim(s) to the claims list
            foreach (var role in roles)
            {
                claims.Add(new Claim("roles", role)); // Use a custom claim name, e.g., "roles"
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _config["Tokens:Issuer"],
                _config["Tokens:Audience"],
                claims,
                signingCredentials: credentials,
                expires: DateTime.UtcNow.AddHours(24)
            );

            return Created("", new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                userName = user.UserName,
                userEmail = user.Email
            });
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateLoginDetails(string id, LoginUpdateViewModel model)
        {
            var loggedInUser = await _userManager.FindByIdAsync(id);

            if (loggedInUser == null)
            {
                return NotFound();
            }

            // Update the email address if provided
            if (!string.IsNullOrEmpty(model.NewEmail))
            {
                loggedInUser.Email = model.NewEmail;
                loggedInUser.UserName = model.UserName; // Set the username to the new email as well
            }

            // Update the password if provided
            if (!string.IsNullOrEmpty(model.NewPassword) && !string.IsNullOrEmpty(model.ConfirmPassword))
            {
                var passwordChangeResult = await _userManager.ChangePasswordAsync(loggedInUser, model.CurrentPassword, model.NewPassword);

                if (!passwordChangeResult.Succeeded)
                {
                    return BadRequest(passwordChangeResult.Errors);
                }
            }

            var updateResult = await _userManager.UpdateAsync(loggedInUser);

            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            return Ok(loggedInUser);
        }


        //Test Authentication
        [HttpGet]
        [Route("testAuth")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<string> SuperSecretThing()
        {
            return "Super secret text has been placed here";
        }


        [HttpGet]
        [Route("testCustAuth")]
        [Authorize(Roles = "Customer")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<string> CustomerAuth()
        {
            return "Super secret text for Customers only";
        }

        [HttpGet]
        [Route("testSuperAuth")]
        [Authorize(Roles = "Superuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<string> SuperAuth()
        {
            return "Super secret text for Superusers only";
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        [HttpGet]
        [Route("testEmployeeAuth")]
        [Authorize(Roles = "Employee")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<string> EmployeeAuth()
        {
            return "Super secret text for Employees only";
        }
    }
}
