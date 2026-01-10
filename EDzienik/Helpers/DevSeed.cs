using EDzienik.Data;
using EDzienik.Entities;
using EDzienik.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EDzienik.Helpers
{
    public static class DevSeed
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            
            foreach (var roleName in Enum.GetNames<UserRoles>())
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            
            const string adminEmail = "admin@edzienik.local";
            const string adminPassword = "Admin123!";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "System"
                };

                var create = await userManager.CreateAsync(admin, adminPassword);
                if (!create.Succeeded)
                {
                    var errors = string.Join(" | ", create.Errors.Select(e => e.Description));
                    throw new Exception("Nie udało się stworzyć admina: " + errors);
                }
            }

            if (!await userManager.IsInRoleAsync(admin, nameof(UserRoles.Admin)))
                await userManager.AddToRoleAsync(admin, nameof(UserRoles.Admin));
        }
    }
}
