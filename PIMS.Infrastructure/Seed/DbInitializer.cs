using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using PIMS.Infrastructure.Identity;

namespace PIMS.Infrastructure.Seed;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider
                               .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
                               .GetRequiredService<UserManager<ApplicationUser>>();

        await SeedRolesAsync(roleManager);
        await SeedAdminAsync(userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Administrator", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@pims.com";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser != null)
            return;

        var user = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, "Admin@123");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Administrator");
        }
    }
}