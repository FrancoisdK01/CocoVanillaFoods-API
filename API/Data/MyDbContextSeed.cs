using API.Model;
using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Linq;

namespace API.Data
{
    public class MyDbContextSeed
    {

        public static async Task SeedUsersAsync(UserManager<User> userManager, MyDbContext context)
        {
            if (!userManager.Users.Any())
            {
                var user = new User
                {
                    DisplayName = "MarindaBloem",
                    Email = "marinda.bloem@promenade.com",
                    UserName = "Marinda",
                    TwoFactorEnabled = false
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
                            PhoneNumber = "0123456789",
                            ID_Number = "0123456789123",
                            Hire_Date = DateTime.Now,
                            UserName = "Marinda"
                        };

                        var customer = new Customer
                        {
                            Id = user.Id,
                            First_Name = "Marinda",
                            Last_Name = "Bloem",
                            Email = "marinda.bloem@promenade.com",
                            PhoneNumber = "0123456789",
                            ID_Number = "0123456789123",
                            UserName = "Marinda",
                            Date_Created = DateTime.Now,
                            Date_of_last_update = DateTime.Now,
                            UserID = user.Id,
                            Gender = "Female",
                            Title = "Mrs",
                            TwoFactorEnabled = false,
                        };

                        // Retrieve the user ID from the Identity database
                        var userIdentity = await context.Users.FindAsync(user.Id);
                        if (userIdentity != null)
                        {
                            superuser.UserID = userIdentity.Id;
                            context.SuperUser.Add(superuser);
                            context.Customers.Add(customer);
                            await context.SaveChangesAsync();
                        }
                    }
                }
                var wineTypes = new List<string> { "Rooi", "Wit" };
                var wineTypesDescriptions = new List<string> { "Dis rooi", "Dis wit" };
                var wineVarietal = new List<string> { "Chenin Blanc", "Pinotage" };
                var wineVarietalDescriptions = new List<string> { "Dis n chenin blanc", "Dis n pinotage" };

                for (int i = 0; i < wineTypes.Count; i++)
                {
                    var addedWineType = new WineType { Name = wineTypes[i], Description = wineTypesDescriptions[i] };
                    context.WineTypes.Add(addedWineType);
                }

                for (int i = 0; i < wineVarietal.Count; i++)
                {
                    var addedVarietal = new Varietal { Name = wineVarietal[i], Description = wineVarietalDescriptions[i] };
                    context.Varietals.Add(addedVarietal);
                }
                await context.SaveChangesAsync();
            }
        }


        public static async Task SeedUserRolesAsync(IServiceProvider serviceProvider, MyDbContext context)
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

                        var aspRole = roleManager.Roles.FirstOrDefault(r => r.Name == roleName);

                        var role = new SystemPrivilege
                        {
                            Id = aspRole.Id,
                            Name = roleName,
                            Description = descriptions[count]
                        };

                        var roleIdentity = await context.Roles.FindAsync(aspRole.Id);
                        if (roleIdentity != null)
                        {
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

