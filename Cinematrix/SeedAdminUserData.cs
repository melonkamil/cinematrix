using System.Security.Claims;
using Cinematrix.Data;
using Microsoft.AspNetCore.Identity;

namespace Cinematrix;

public static class SeedAdminUserData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var roleExist = await roleManager.RoleExistsAsync("Admin");

        if (!roleExist)
        {
            var adminRole = new IdentityRole("Admin");
            await roleManager.CreateAsync(adminRole);

            await roleManager.AddClaimAsync(adminRole, new Claim("Permission", "ManageMovies"));
        }

        var user = await userManager.FindByEmailAsync("admin@cinematrix.com");
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = "admin@cinematrix.com",
                Email = "admin@cinematrix.com"
            };

            var result = await userManager.CreateAsync(user, "P@ssw0rd");
            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                await userManager.UpdateAsync(user);
                await userManager.AddToRoleAsync(user, "Admin");
            }
        }
    }
}