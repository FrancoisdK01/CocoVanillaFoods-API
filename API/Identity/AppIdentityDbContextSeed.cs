﻿using API.Data;
using API.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Runtime.CompilerServices;

namespace API.Identity
{
    public class AppIdentityDbContextSeed
    {
        private readonly MyDbContext _context;

        public static async Task SeedUsersAsync(UserManager<User> userManager, MyDbContext context, AppIdentityDbContext identityDbContext)
        {
            if (!userManager.Users.Any())
            {
                var user = new User
                {
                    DisplayName = "MarindaBloem",
                    Email = "marinda.bloem@promenade.com",
                    UserName = "Marinda"
                };

                var createUserResult = await userManager.CreateAsync(user, "P@ssword1");
                await context.Users.AddAsync(user);

                if (createUserResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Superuser");
                    await userManager.AddToRoleAsync(user, "Customer");

                    if (!context.SuperUser.Any())
                    {
                        var superuser = new SuperUser
                        {
                            Id = user.Id,
                            First_Name = "Marinda",
                            Last_Name = "Bloem",
                            Email = "marinda.bloem@promenade.com",
                            PhoneNumber = "1234567890",
                            ID_Number = "0123456789123",
                            Hire_Date = DateTime.Now,
                            UserName = "Marinda"
                        };

                        // Retrieve the user ID from the Identity database
                        var userIdentity = await identityDbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
                        if (userIdentity != null)
                        {
                            superuser.UserID = userIdentity.Id;
                            context.SuperUser.Add(superuser);
                            await context.SaveChangesAsync();
                        }
                    }
                }
            }
        }


        public static async Task SeedUserRolesAsync(IServiceProvider serviceProvider, MyDbContext context, AppIdentityDbContext identityDbContext)
        {
            try
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var roles = new List<string> { "Superuser", "Admin", "Employee", "Customer" };
                var descriptions = new List<string> { "Access to the entire system", "Access to the wine and events system", "Access to the inventory system", "Access to the customer system" };
                var count = 0;
                foreach (var roleName in roles)
                {
                    var roleExists = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                        var role = new SystemPrivilege
                        {
                            Name = roleName,
                            Privilege_Description = descriptions[count]
                        };

                        var roleIdentity = await identityDbContext.Roles.FirstOrDefaultAsync(x => x.Name == role.Name);
                        if (roleIdentity != null)
                        {
                            role.RoleId = roleIdentity.Id;
                            context.SystemPrivileges.Add(role);
                            await context.SaveChangesAsync();
                        }
                        count++;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}