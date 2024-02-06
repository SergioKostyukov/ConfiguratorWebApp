using ConfiguratorWebApp.Data;
using ConfiguratorWebApp.Models.Entites;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ConfiguratorWebApp.Services;

namespace ConfiguratorWebApp;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        // Add services to the container.
        services.AddControllersWithViews();
        services.AddScoped<ConfigurationService>();

        services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(_configuration.GetConnectionString("ConfigurationApp")));
    }

    // Method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
                name: "configuration",
                pattern: "",
                defaults: new { controller = "Configuration", action = "GetConfigurationTree" });
        });
    }
}