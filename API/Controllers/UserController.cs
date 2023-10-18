using API.Data;
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
        public async Task<ActionResult<object>> UserLogin(LoginViewModel lvm)
        {
            var user = await _userManager.FindByEmailAsync(lvm.email);

            if (user != null && await _userManager.CheckPasswordAsync(user, lvm.password))
            {
                if (await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    // Generate and send the 2FA code via email
                    var code = await _userManager.GenerateTwoFactorTokenAsync(user, "email");
                    UserSend2FACodeByEmail(user, code);


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
                        var expirationTime = tokenResult.Expiration;

                        return Ok(new { tokenValue, userNameValue, userEmailValue, expirationTime, twoFactorEnabled = false });

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
        public async Task<ActionResult<UserViewModel>> UserRegister(RegisterViewModel rvm)
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

                if (result.Succeeded)
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
                    return BadRequest("Failed to add customer details to the database");
                }
            }
            else
            {
                return Forbid("Account already exists.");
            }
        }

        [HttpPost]
        [Route("Logout")]   
        public async Task<IActionResult> UserLogout()
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
            // Fetch the customer details for this user
            var customer = _context.Customers.FirstOrDefault(c => c.UserID == user.Id);

            var roles = _userManager.GetRolesAsync(user).Result;

            // Create JWT Token
            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            // Add phone number if customer is not null and has a phone number
            if (customer != null && !string.IsNullOrEmpty(customer.PhoneNumber))
            {
                claims.Add(new Claim("phoneNumber", customer.PhoneNumber));
            }

            // Add the role claim(s) to the claims list
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(24); // KEEP THIS 30, SYSTEM WORKS DYNAMICALLY
            var token = new JwtSecurityToken(
                _config["Tokens:Issuer"],
                _config["Tokens:Audience"],
                claims,
                signingCredentials: credentials,
                expires: expiration
            );

            return new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), user.UserName, user.Email, expiration);
        }

        //2FA code Generator
        private async void UserSend2FACodeByEmail(User user, string code)
        {
            // Generate the email message with the code
            var evm = new EmailViewModel
            {
                To = user.Email,
                Subject = "2-Factor Authentication code: " + code,
                Body = $@"Below you will find your 2-Factor authentication code, please use this code to access your account
                            Code: {code}
                            Kind regards,
                            The Promenade Team
                            "
            };
            await _emailService.SendSimpleMessage(evm);
            //_emailService.SendEmail(evm);
        }

        [HttpGet]
        [Route("GetUserByEmail/{email}")]
        public async Task<IActionResult> GetSingleUserIdByEmail(string email)
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
        public async Task<IActionResult> ValidateCode(VerifyCodeViewModel model)
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
                            var expirationTime = tokenResult.Expiration;

                            return Ok(new { tokenValue, userNameValue, userEmailValue, expirationTime, twoFactorEnabled = true });
                        }
                        else
                        {
                            return BadRequest("Token failed to generate");
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
        [DynamicAuthorize]
        public async Task<IActionResult> UserUpdateLoginDetails(string id, LoginUpdateViewModel model)
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

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> UserForgotPassword(ForgotPasswordViewModel model)
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
                    Reset Password
                    You have requested to reset your password.
                    Please follow the following steps to update your login details:
                    
                    Find your login details below these steps
                    Go to our website and login your account with the details provided
                    If you wish to update your login details instead of keeping this password: Go to the account page
                    Click on the Username and password tab in die sidebar
                    Update your details and log into your account with your updated details
                    

                    You updated login details follow
                    Email: {user.Email}
                    Password: {newPassword}
                    
                    If you did not request a password reset, please ignore this email.
                    Kind regards,
                    The Promenade Team
                "
            };

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                await _emailService.SendSimpleMessage(evm);
                //_emailService.SendEmail(evm);
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

            public DateTime Expiration { get; }

            public TokenResult(string token, string userName, string userEmail, DateTime expiration)
            {
                Token = token;
                UserName = userName;
                UserEmail = userEmail;
                Expiration = expiration;
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

        //////////////////////////Marco se code om die gender chart inligting te kry vir Charts ////////////////////////////////////////
        [HttpGet]
        [Route("GetGenderDistribution")]
        [DynamicAuthorize]
        public IActionResult GenerateGenderDistribution()
        {
            var genderDistribution = _context.Customers.GroupBy(c => c.Gender)
                                                      .Select(g => new { Gender = g.Key, Count = g.Count() })
                                                      .ToList();

            return Ok(genderDistribution);
        }


    }
}
