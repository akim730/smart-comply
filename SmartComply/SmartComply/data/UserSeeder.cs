using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SmartComply.Constant;

namespace SmartComply.data
{
    public class UserSeeder
    {
        public static async Task SeedUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            var users = new[]
            {
                (
                    Email: config["SeedUsers:SuperAdmin:Email"]!,
                    Password: config["SeedUsers:SuperAdmin:Password"]!,
                    Role: Roles.superAdmin
                ),
                (
                    Email: config["SeedUsers:Admin:Email"]!,
                    Password: config["SeedUsers:Admin:Password"]!,
                    Role: Roles.Admin
                ),
                (
                    Email: config["SeedUsers:Manager:Email"]!,
                    Password: config["SeedUsers:Manager:Password"]!,
                    Role: Roles.Manager
                ),
                (
                    Email: config["SeedUsers:Auditor:Email"]!,
                    Password: config["SeedUsers:Auditor:Password"]!,
                    Role: Roles.Auditor
                ),
            };

            foreach (var (email, password, role) in users)
            {
                await CreateUserWithRole(userManager, email, password, role);
            }
        }

        private static async Task CreateUserWithRole(
            UserManager<IdentityUser> userManager,
            string email,
            string password,
            string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new IdentityUser
                {
                    Email = email,
                    EmailConfirmed = true,
                    UserName = email
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    throw new Exception(
                        $"Failed creating user {email}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}"
                    );
                }
            }
        }
    }
}
