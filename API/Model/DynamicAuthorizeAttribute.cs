using API.Data;
using Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

[AttributeUsage(AttributeTargets.Method)]
public class DynamicAuthorizeAttribute : TypeFilterAttribute
{
    public DynamicAuthorizeAttribute() : base(typeof(DynamicAuthorizeFilter))
    {
    }

    private class DynamicAuthorizeFilter : IAuthorizationFilter
    {
        private readonly MyDbContext _context;
        private readonly ILogger _logger;

        // Constructor injection to get the DbContext
        public DynamicAuthorizeFilter(MyDbContext context, ILogger<DynamicAuthorizeFilter> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string authHeader = context.HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(authHeader))
            {
                _logger.LogWarning("Token is missing from the request headers.");
                context.Result = new ForbidResult();
                return;
            }

            string token = authHeader.Replace("Bearer ", "");
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(token);
            var userNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
            var userEmailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            string controllerName = "";
            string methodName = "";
            if (context.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
            {
                controllerName = actionDescriptor.ControllerName+"Controller";
                methodName = actionDescriptor.ActionName;
            }

            var requiredPrivileges = GetRequiredPrivilegesForMethod(controllerName, methodName);
            var userPrivilegeIds = GetUserPrivileges(userNameClaim); // Ensure this method returns Ids

            if (!requiredPrivileges.Any(requiredPrivilege => userPrivilegeIds.Contains(requiredPrivilege)))
            {
                context.Result = new ForbidResult();
            }
        }

        private List<string> GetRequiredPrivilegesForMethod(string controllerName, string methodName)
        {
            try
            {
                // Assuming that each methodName can have multiple SystemPrivilegeIds
                return _context.MethodPrivilegeMappings
                               .Where(m => m.ControllerName == controllerName && m.MethodName == methodName)
                               .Select(m => m.SystemPrivilegeId)
                               .ToList();
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine(ex);
                // Return an empty list or throw an exception depending on your use case
                return new List<string>();
            }
        }

        private List<string> GetUserPrivileges(string username)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserName == username);
                if (user == null)
                {
                    // Log error or throw exception
                    Console.WriteLine($"User with username {username} not found.");
                    return new List<string>();
                }

                var roleIds = _context.UserRoles.Where(ur => ur.UserId == user.Id).Select(ur => ur.RoleId).ToList();
                if (roleIds.Count == 0)
                {
                    // Log error or throw exception
                    Console.WriteLine($"No roles found for user with username {username}.");
                    return new List<string>();
                }

                var privilegeIds = _context.SystemPrivileges.Where(sp => roleIds.Contains(sp.Id)).Select(sp => sp.Id).ToList();
                return privilegeIds;
            }
            catch (Exception ex)
            {
                // Log exception details
                Console.WriteLine(ex);
                // Return an empty list or throw an exception depending on your use case
                return new List<string>();
            }
        }

    }
}
