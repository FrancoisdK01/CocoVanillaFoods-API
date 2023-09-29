using API.Data;
using API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using Twilio;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            );

            services.AddDbContext<MyDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())));

            var twilioSettings = Configuration.GetSection("Twilio");
            TwilioClient.Init(twilioSettings["AccountSid"], twilioSettings["AuthToken"]);

            services.AddHostedService<BackupHostedService>();
            services.AddSingleton<IBackupService, BackupService>();

            services.AddDataProtection();

            services.AddHttpClient();

            services.AddDirectoryBrowser();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Global exception handling
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = 500; // Internal Server Error
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        // Log the exception here
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            error = "An unexpected error occurred."
                        }));
                    }
                });
            });

            app.UseHttpsRedirection();

            // Use static files before UseRouting
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // ... other endpoints
            });
        }
    }
}
