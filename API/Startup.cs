using Microsoft.AspNetCore.Builder;

namespace API
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDirectoryBrowser();
            // ...

            services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 104857600; // Set the maximum request body size (e.g., 100MB)
            });

            // ...
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // ...

            app.UseStaticFiles(); // Add this line to enable serving static files

            // ...
        }
    }
}
