using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Bookano.Infrastructure.Persistence.Seeds
{
    public static class DatabaseInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await DefaultRoles.SeedAsync(roleManager);

            await DefaultUsers.SeedAdminUserAsync(
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>()
            );
        }
    }
}
