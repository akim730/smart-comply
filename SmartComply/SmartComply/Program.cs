using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartComply.Constant;
using SmartComply.data;
using System.Configuration; // Add this using directive for ConfigurationManager (if needed, though builder.Configuration is preferred)

namespace SmartComply
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Conditional DB Context Configuration based on Environment
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("Database")); // Use local for Development
                });
            }
            else // This will cover Production, Staging, etc.
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("Productiondb")); // Use remote for Production
                });
            }

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Moved UseHttpsRedirection above UseStaticFiles/UseRouting typically
            app.UseHttpsRedirection();
            app.UseStaticFiles(); // Added this, important for static assets like CSS/JS
            app.UseRouting();

            // Seeding logic (ensure this is safe to run on production or handle appropriately)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                // Consider adding a check to ensure migrations are applied before seeding if not done manually
                // var context = services.GetRequiredService<ApplicationDbContext>();
                // context.Database.Migrate(); // Only if you want to apply migrations at startup (might need permissions)

                // Role/User Seeding
                // Be cautious with seeding on production if it's not idempotent or needs to run only once
                RoleSeeder.SeedRolesAsync(services).Wait();
                UserSeeder.SeedUserAsync(services).Wait();
            }

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

           // app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Admin}/{action=Index}/{id?}");
                //.WithStaticAssets();

            app.Run();
        }
    }
}