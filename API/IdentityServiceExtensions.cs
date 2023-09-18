using API.Data;
using API.Model;
using Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace API
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
            IConfiguration config) 
        {

            services.AddDbContext<MyDbContext>(opt => opt.UseSqlServer(config.GetConnectionString("MyConnection")));

            services.AddIdentity<User, IdentityRole>().
                AddEntityFrameworkStores<MyDbContext>().
                AddSignInManager<SignInManager<User>>().
                AddDefaultTokenProviders().
                AddTokenProvider<EmailTokenProvider<User>>("email");

            services.AddScoped<IEmailService, EmailService>();
            //services.AddIdentityCore<User>(opt =>
            //{
            //}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>().AddSignInManager<SignInManager<User>>().AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Tokens:Issuer"],
                    ValidAudience = config["Tokens:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Tokens:Key"])),
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromMinutes(0)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication Failed");
                        Console.WriteLine(context.ToString());
                        return Task.CompletedTask;
                    }
                };
            });
            services.AddAuthorization();

            return services;
        }
    }
}
