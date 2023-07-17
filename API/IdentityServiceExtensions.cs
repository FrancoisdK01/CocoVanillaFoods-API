using API.Data;
using API.Identity;
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
            services.AddDbContext<AppIdentityDbContext>(opt =>
            {
                opt.UseSqlServer(config.GetConnectionString("IdentityConnection"));
            });

            services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>().AddSignInManager<SignInManager<User>>().AddDefaultTokenProviders().AddTokenProvider<EmailTokenProvider<User>>("email");
            services.AddScoped<IEmailService, EmailService>();
            //services.AddIdentityCore<User>(opt =>
            //{
            //}).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppIdentityDbContext>().AddSignInManager<SignInManager<User>>().AddDefaultTokenProviders();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Tokens:Key"])),
                    ValidIssuer = config["Tokens:Issuer"],
                    ValidAudience = config["Tokens:Audience"],
                    ValidateIssuer = true
                };
            });
            services.AddAuthorization();

            return services;
        }
    }
}
