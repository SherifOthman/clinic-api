using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Data.Seeders;

public static class UserClinicSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        // Get all users who have a clinic but don't have UserClinic entries
        var usersWithClinics = await context.Users
            .Where(u => u.ClinicId != null)
            .Where(u => !context.UserClinics.Any(uc => uc.UserId == u.Id && uc.ClinicId == u.ClinicId))
            .ToListAsync();

        if (!usersWithClinics.Any())
        {
            logger.LogInformation("All users already have UserClinic entries");
            return;
        }

        var userClinics = new List<UserClinic>();
        
        foreach (var user in usersWithClinics)
        {
            var userClinic = new UserClinic
            {
                UserId = user.Id,
                ClinicId = user.ClinicId!.Value,
                IsOwner = true, // Assume existing users are owners of their clinics
                IsActive = true,
                JoinedAt = user.CreatedAt
            };
            
            userClinics.Add(userClinic);
            
            // Set CurrentClinicId if not already set
            if (user.CurrentClinicId == null)
            {
                user.CurrentClinicId = user.ClinicId;
            }
        }

        context.UserClinics.AddRange(userClinics);
        await context.SaveChangesAsync();

        logger.LogInformation("Created {Count} UserClinic entries for existing users", userClinics.Count);
    }
}