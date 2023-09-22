using API.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Controller;
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
                            ID_Number = "7001030203088",
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
                            ID_Number = "7001030203088",
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
                var wineTypes = new List<string> { "Red", "White" };
                var wineTypesDescriptions = new List<string> { "Red in colour", "White in colour" };
                var wineVarietal = new List<string> { "Chenin Blanc", "Pinotage", "Pinotage / Cabernet Savignon / Merlot and Shiraz" };
                var wineVarietalBlends = new List<Boolean> { false, false, true };
                var wineVarietalDescriptions = new List<string> { "Chenin blanc cultivar", "Pinotage cultivar", "Cape Blend" };
                var wineVarietalWineTypes = new List<int> { 2, 1, 1 };

                for (int i = 0; i < wineTypes.Count; i++)
                {
                    var addedWineType = new WineType { Name = wineTypes[i], Description = wineTypesDescriptions[i] };
                    context.WineTypes.Add(addedWineType);
                }

                await context.SaveChangesAsync();

                for (int i = 0; i < wineVarietal.Count; i++)
                {
                    var addedVarietal = new Varietal { Name = wineVarietal[i], Description = wineVarietalDescriptions[i], WineTypeID = wineVarietalWineTypes[i], Blend = wineVarietalBlends[i]};
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

        public static async Task SeedMethodPrivilegeMappingsAsync(MyDbContext context)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "API");
            var controllerType = typeof(ControllerBase);

            var superuserPrivilegeId = context.SystemPrivileges.FirstOrDefault(p => p.Name == "Superuser")?.Id;
            var adminPrivilegeId = context.SystemPrivileges.FirstOrDefault(p => p.Name == "Admin")?.Id;
            var employeePrivilegeId = context.SystemPrivileges.FirstOrDefault(p => p.Name == "Employee")?.Id;
            var customerPrivilegeId = context.SystemPrivileges.FirstOrDefault(p => p.Name == "Customer")?.Id;

            // Ensure the privilege IDs are found.
            if (superuserPrivilegeId == null || adminPrivilegeId == null || employeePrivilegeId == null || customerPrivilegeId == null)
            {
                throw new Exception("Required privilege not found.");
            }

            var adminControllers = new[]
            {
                "BlacklistController", "ChatBotController", "DiscountsController", "EarlyBirdsController",
                "EventPriceController", "EventsController", "EventTypeController", "HelpResourceController",
                "MailController", "SupplierOrdersController", "VarietalsController", "WinesController", "WineTypesController"
            };

            var employeeControllers = new[]
            {
                "ChatBotController", "HelpResourceController", "InventoryController", "MailController",
                "StockTakeController", "SupplierOrdersController", "SuppliersController", "VarietalsController",
                "WineTypesController", "WinesController", "WriteOffsController", "WriteOff_ReasonController"
            };

            var customerControllersWithMethods = new Dictionary<string, List<string>>
            {
                {"CartController", new List<string> {"GetCart", "AddToCart", "IncrementCartItemQuantity", "DecrementCartItemQuantity", "GetCartTotal", "ApplyDiscount", "ClearCart"}},
                {"ChatBotController", new List<string> { "SendMessageToBot" }},
                {"CustomersController", new List<string> { "GetCustomer", "PutCustomer" }},
                {"DiscountsController", new List<string> {"ValidateDiscountCode"}},
                {"EventsController", new List<string> {"GetEvents", "GetEvent", "PurchaseTicket" }},
                //{"FAQsController", new List<string> {"GetFAQs", "GetFAQ" }},
                {"OrderHistoryController", new List<string> { "CreateOrder", "GetOrdersForUser", "GetOrder" }},
                {"PaymentController", new List<string> {"CreatePayment", "HandlePaymentResult"}},
                {"RefundsController", new List<string> { "GetRefundRequests", "GetRefundReponses", "GetResponse", "RequestRefund", "GetWineDetailsForRefund", "GetUserRefundRequests"}},
                {"TicketPurchasesController", new List<string> { "PostTicket", "GetTicketPurchase", "GetPurchasesForUser" }},
                {"UserController", new List<string> { "GetUserIdByEmail", "UpdateLoginDetails" }},
                {"WinesController", new List<string> {"GetWinesForCustomers"}},
                {"WishlistController", new List<string> { "GetWishlist", "AddToWishlist", "RemoveFromWishlist" }}
            };

            var mappings = new List<MethodPrivilegeMapping>();

            foreach (var type in assembly.GetTypes().Where(t => t.IsSubclassOf(controllerType)))
            {
                foreach (var method in type.GetMethods().Where(m => m.IsPublic && m.DeclaringType == type))
                {
                    if (adminControllers.Contains(type.Name))
                    {
                        mappings.Add(new MethodPrivilegeMapping
                        {
                            ControllerName = type.Name,
                            MethodName = method.Name,
                            SystemPrivilegeId = adminPrivilegeId
                        });
                    }
                    if (employeeControllers.Contains(type.Name))
                    {
                        mappings.Add(new MethodPrivilegeMapping
                        {
                            ControllerName = type.Name,
                            MethodName = method.Name,
                            SystemPrivilegeId = employeePrivilegeId
                        });
                    }

                    if (customerControllersWithMethods.TryGetValue(type.Name, out var allowedMethods) && allowedMethods.Contains(method.Name))
                    {
                        mappings.Add(new MethodPrivilegeMapping
                        {
                            ControllerName = type.Name,
                            MethodName = method.Name,
                            SystemPrivilegeId = customerPrivilegeId
                        });
                    }


                    mappings.Add(new MethodPrivilegeMapping
                    {
                        ControllerName = type.Name,
                        MethodName = method.Name,
                        SystemPrivilegeId = superuserPrivilegeId
                    });

                }
            }

            if (!context.MethodPrivilegeMappings.Any())
            {
                await context.MethodPrivilegeMappings.AddRangeAsync(mappings);
                await context.SaveChangesAsync();
            }
        }




        public static async Task SeedHelpResource(MyDbContext context, IWebHostEnvironment _webHostEnvironment)
        {
            // Check if there's already an entry in the HelpResources table
            if (!context.HelpResources.Any())
            {
                var resource = new HelpResource
                {
                    videoPath = "/assets/helpVideo.MP4",
                    pdfPath = "/assets/helpDocument.pdf"
                };

                context.HelpResources.Add(resource);
                await context.SaveChangesAsync();
            }
        }
    }
}

