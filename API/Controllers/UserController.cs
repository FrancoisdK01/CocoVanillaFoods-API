﻿using API.Data;
using API.Model;
using API.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System;
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
        private static readonly Random random = new Random();
        private readonly IEmailService _emailService;

        public UserController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration, MyDbContext context, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = configuration;
            _context = context;
            _emailService = emailService;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult<object>> Login(LoginViewModel lvm)
        {
            var user = await _userManager.FindByEmailAsync(lvm.email);

            if (user != null && await _userManager.CheckPasswordAsync(user, lvm.password))
            {
                if (await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    // Generate and send the 2FA code via email
                    var code = await _userManager.GenerateTwoFactorTokenAsync(user, "email");
                    Send2FACodeByEmail(user, code);


                    // Return a response indicating 2FA is enabled
                    return Ok(new { message = "Two-factor authentication code has been sent to your email.", twoFactorEnabled = true });


                }
                else
                {
                    var tokenResult = GenerateJWTToken(user);

                    if (tokenResult != null)
                    {

                        var tokenValue = tokenResult.Token;
                        var userNameValue = tokenResult.UserName;
                        var userEmailValue = tokenResult.UserEmail;

                        return Ok(new { tokenValue, userNameValue, userEmailValue, twoFactorEnabled = false });

                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error. Please contact support.");
                    }
                }
            }
            else
            {
                return NotFound(new { error = "User not found or invalid credentials." });
            }
        }


        [HttpPost]
        [Route("Register")]
        public async Task<ActionResult<UserViewModel>> Register(RegisterViewModel rvm)
        {
            var user = await _userManager.FindByEmailAsync(rvm.Email);
            var cust = await _context.Customers.FirstOrDefaultAsync(x => x.Email == rvm.Email);

            if (user == null && cust == null)
            {
                user = new User
                {
                    UserName = rvm.DisplayName,
                    Email = rvm.Email,
                    DisplayName = rvm.DisplayName,
                    TwoFactorEnabled = rvm.EnableTwoFactorAuth
                };

                var customer = new Customer
                {
                    First_Name = rvm.FirstName,
                    Last_Name = rvm.LastName,
                    Email = rvm.Email,
                    PhoneNumber = rvm.PhoneNumber,
                    ID_Number = rvm.IDNumber,
                    Date_Created = DateTime.Now,
                    UserID = user.Id,
                    Title = rvm.Title,
                    Gender = rvm.Gender,
                    Date_of_last_update = DateTime.Now,
                    TwoFactorEnabled = rvm.EnableTwoFactorAuth
                };
                var result = await _userManager.CreateAsync(user, rvm.Password);

                //if (rvm.EnableTwoFactorAuth)
                //{
                //    // Generate 2FA code and send it via email
                //    var code = await _userManager.GenerateTwoFactorTokenAsync(user, "email");
                //    Send2FACodeByEmail(user, code);
                //}

                if (result.Succeeded)
                {
                    _context.Users.Add(user);
                    var userSavedChanges = await _context.SaveChangesAsync();

                    if (userSavedChanges > 0)
                    {
                        _context.Customers.Add(customer);
                        var customerSavedChanges = await _context.SaveChangesAsync();

                        if (customerSavedChanges > 0)
                        {
                            await _userManager.AddToRoleAsync(user, "Customer");

                            return Ok("Your account has been created!");
                        }
                        else
                        {
                            var reverseAction = await _userManager.FindByEmailAsync(user.Email);
                            await _userManager.DeleteAsync(reverseAction);
                        }
                    }
                    else
                    {
                        return BadRequest("Failed to add customer details to the database");
                    }
                }
                else
                {
                    var reverseUser = await _userManager.FindByEmailAsync(user.Email);
                    var reverseCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                    if (reverseUser != null && reverseCustomer != null)
                    {
                        _context.Customers.Remove(reverseCustomer);
                        await _context.SaveChangesAsync();
                        await _userManager.DeleteAsync(reverseUser);
                    }

                    return StatusCode(StatusCodes.Status500InternalServerError, "Creating the user account failed, please contact support");
                }
            }
            else
            {
                return Forbid("Account already exists.");
            }
            return Ok("Your account has been created!");
        }

        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Perform logout actions

                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                var token = new
                {
                    tokenValue = ""
                };

                return Ok(new { token });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error. Please contact support.");
            }
        }




        [HttpGet]
        private TokenResult GenerateJWTToken(User user)
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
                claims.Add(new Claim(ClaimTypes.Role, role));
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

            return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), user.UserName, user.Email);
        }

        //2FA code Generator
        private void Send2FACodeByEmail(User user, string code)
        {
            // Generate the email message with the code
            var evm = new EmailViewModel
            {
                To = user.Email,
                Subject = "2-Factor Authentication code",
                Body = $@"
                                        <h5>Below you will find your 2-Factor authentication code, please use this code to access your account</h5>
                                        
                                        <ul>
                                            <li>Code: {code}</li>
                                        </ul>

                                        <p>Kind regards,</p>
                                        <p>The Promenade Team</p>
                                        "
            };

            _emailService.SendEmail(evm);
        }

        [HttpGet]
        [Route("GetUserByEmail/{email}")]
        public async Task<IActionResult> GetUserIdByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound("The user you are searching for doesn't exist");
            }
            else
            {
                var userId = user.Id;
                return Ok(new { userId });
            }
        }

        [HttpPost]
        [Route("VerifyCode")]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user != null)
            {
                var isCodeValid = await _userManager.VerifyTwoFactorTokenAsync(user, "email", model.Code);
                if (isCodeValid)
                {
                    try
                    {
                        var tokenResult = GenerateJWTToken(user);

                        if (tokenResult != null)
                        {
                            var tokenValue = tokenResult.Token;
                            var userNameValue = tokenResult.UserName;
                            var userEmailValue = tokenResult.UserEmail;

                            return Ok(new { tokenValue, userNameValue, userEmailValue, twoFactorEnabled = true });
                        }
                        else
                        {
                            return BadRequest("Boo");
                        }
                    }
                    catch (Exception)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Internal Server Error. Please contact support.");
                    }
                }
                else
                {
                    return BadRequest("Invalid verification code.");
                }
            }
            else
            {
                return NotFound("User not found.");
            }
        }


        [HttpPut]
        [Route("UpdateLoginDetails/{id}")]
        [AllowAnonymous]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> UpdateLoginDetails(string id, LoginUpdateViewModel model)
        {
            var loggedInUser = await _userManager.FindByIdAsync(id);
            var loggedInCust = await _context.Customers.FirstOrDefaultAsync(x => x.UserID == id);

            if (loggedInUser == null && loggedInCust == null)
            {
                return NotFound();
            }
            else
            {
                if (!string.IsNullOrEmpty(model.NewEmail))
                {
                    loggedInUser.Email = model.NewEmail;
                    loggedInUser.UserName = model.UserName;
                    loggedInCust.Email = model.NewEmail;
                    loggedInCust.Date_of_last_update = DateTime.UtcNow;
                    loggedInCust.UserName = model.UserName;
                    loggedInUser.DisplayName = model.UserName;
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
                _context.Update(loggedInCust);
                _context.SaveChanges();
                if (!updateResult.Succeeded)
                {
                    return BadRequest(updateResult.Errors);
                }

                return Ok(loggedInUser);
            }
        }

        // ASSIGN AND REMOVE ROLES

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
                    IsCustomer = roles.Contains("Customer"),
                    IsEmployee = roles.Contains("Employee"),
                    IsAdmin = roles.Contains("Admin"),
                    IsSuperUser = roles.Contains("Superuser")
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

            // Update user roles based on the checkbox selections
            if (model.IsCustomer)
            {
                await _userManager.AddToRoleAsync(user, "Customer");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "Customer");
            }

            if (model.IsEmployee)
            {
                await _userManager.AddToRoleAsync(user, "Employee");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "Employee");
            }

            if (model.IsAdmin)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }

            if (model.IsSuperUser)
            {
                await _userManager.AddToRoleAsync(user, "Superuser");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(user, "Superuser");
            }

            return Ok();
        }


        // AUTH CHECKING
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
        [Route("testEmployeeAuth")]
        [Authorize(Roles = "Employee")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public ActionResult<string> EmployeeAuth()
        {
            return "Super secret text for Employees only";
        }


        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            var newPassword = GeneratePassword();
            if (user == null)
            {
                // User with the provided email does not exist
                return NotFound();
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var evm = new EmailViewModel
            {
                To = user.Email,
                Subject = "Reset Password",
                Body = $@"
                    <h4>Reset Password</h4>
                    <h6>You have requested to reset your password.</h6>
                    <p>Please follow the following steps to update your login details:</p>
                    <ol>
                        <li>Find your login details below these steps</li>
                        <li>Go to our website and login your account with the details provided</li>
                        <li>If you wish to update your login details instead of keeping this password: Go to the account page</li>
                        <li>Click on the Username and password tab in die sidebar</li>
                        <li>Update your details and log into your account with your updated details</li>
                    </ol>

                    <p> You updated login details follow </p>
                    <ul>
                        <li>Email: {user.Email}</li>
                        <li>Password: {newPassword}</li>
                    </ul>
                    <p>If you did not request a password reset, please ignore this email.</p>
                    <p>Kind regards,</p>
                    <p>The Promenade Team</p>
                "
            };

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                _emailService.SendEmail(evm);
            }
            else
            {
                return BadRequest("Failed to reset password");
            }
            return Ok();
        }

        private class TokenResult
        {
            public string Token { get; }
            public string UserName { get; }
            public string UserEmail { get; }

            public TokenResult(string token, string userName, string userEmail)
            {
                Token = token;
                UserName = userName;
                UserEmail = userEmail;
            }
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
                {
                    string allCharacters = uppercaseLetters + specialCharacters + digits + lowercaseLetters;
                    password[i] = GetRandomCharacter(allCharacters);
                }
            }

            // Shuffle the password characters
            for (int i = 0; i < 12; i++)
            {
                {
                    int randomIndex = random.Next(i, 12);
                    char temp = password[randomIndex];
                    password[randomIndex] = password[i];
                    password[i] = temp;
                }
            }

            return new string(password);
        }


        private static char GetRandomCharacter(string characterSet)
        {
            {
                int randomIndex = random.Next(0, characterSet.Length);
                return characterSet[randomIndex];
            }
        }
    }
}
