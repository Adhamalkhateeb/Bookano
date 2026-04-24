using Microsoft.AspNetCore.Identity;

namespace Bookano.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManger)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@bookano.com",
                FullName = "Admin",
                EmailConfirmed = true,
            };
            var user = await userManger.FindByEmailAsync(admin.Email);

            if (user is null)
            {
                await userManger.CreateAsync(admin, "P@ssword1504");
                await userManger.AddToRoleAsync(admin, AppRoles.Admin);
            }
        }
    }
}
