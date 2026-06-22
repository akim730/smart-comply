using Microsoft.AspNetCore.Identity;

using SmartComply.Constant;



namespace SmartComply.data

{

    public class UserSeeder

    {

        public static async Task SeedUserAsync(IServiceProvider serviceProvider)

        {

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();



            await CreateUserWithRole(userManager, "superAdmin@gmail.com", "superAdmin123!", Roles.superAdmin);

            await CreateUserWithRole(userManager, "admin@gmail.com", "Admin123!", Roles.Admin);

            await CreateUserWithRole(userManager, "manager@gmail.com", "Manager123!", Roles.Manager);

            await CreateUserWithRole(userManager, "auditor@gmail.com", "Auditor123!", Roles.Auditor);

        }



        private static async Task CreateUserWithRole(UserManager<IdentityUser> userManager, string email, string password, string role)

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

                    throw new Exception($"Failes creating user with {user.Email}. Error: {string.Join(",", result.Errors)}");

                }

            }

        }

    }

}