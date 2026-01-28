using ClinicManagement.Domain.Common.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Seeders;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
    {
        var roles = new[]
        {
            RoleNames.Admin,
            RoleNames.ClinicOwner,
            RoleNames.Doctor,
            RoleNames.Receptionist
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole<int>(roleName);
                await roleManager.CreateAsync(role);
            }
        }
    }
}