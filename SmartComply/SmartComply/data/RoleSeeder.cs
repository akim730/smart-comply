using Microsoft.AspNetCore.Identity;
using SmartComply.Constant;

namespace SmartComply.data
{
    public class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(Roles.Admin))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            }

            if (!await roleManager.RoleExistsAsync(Roles.superAdmin))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.superAdmin));
            }

            if (!await roleManager.RoleExistsAsync(Roles.Manager))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Manager));
            }

            if (!await roleManager.RoleExistsAsync(Roles.Auditor))
            {
                await roleManager.CreateAsync(new IdentityRole(Roles.Auditor));
            }
        }
    }
}
