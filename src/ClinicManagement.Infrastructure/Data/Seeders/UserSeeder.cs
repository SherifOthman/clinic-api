using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ClinicManagement.Infrastructure.Data.Seeders;

public static class UserSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<User> userManager,
        IFileSystem fileSystem,
        ILogger logger)
    {
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Users already exist, skipping user seeding");
            return;
        }

        try
        {
            var jsonPath = fileSystem.Combine(fileSystem.GetBaseDirectory(), "SeedData", "users.json");
            if (!fileSystem.Exists(jsonPath))
            {
                logger.LogWarning("Users seed data file not found at {Path}", jsonPath);
                return;
            }

            var jsonContent = await fileSystem.ReadAllTextAsync(jsonPath);
            var userData = JsonSerializer.Deserialize<List<UserSeedData>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (userData == null || !userData.Any())
            {
                logger.LogWarning("No user data found in seed file");
                return;
            }

            foreach (var userSeed in userData)
            {
                var user = new User
                {
                    FirstName = userSeed.FirstName,
                    LastName = userSeed.LastName,
                    UserName = userSeed.Username,
                    Email = userSeed.Email,
                    PhoneNumber = userSeed.PhoneNumber,
                    EmailConfirmed = userSeed.EmailConfirmed,
                    CreatedAt = userSeed.CreatedAt
                };

                var result = await userManager.CreateAsync(user, userSeed.Password);
                if (result.Succeeded)
                {
                    // Add roles
                    if (userSeed.Roles?.Any() == true)
                    {
                        await userManager.AddToRolesAsync(user, userSeed.Roles);
                    }

                    logger.LogInformation("Created user: {Username} ({Email}) with roles: {Roles}", 
                        user.UserName, user.Email, string.Join(", ", userSeed.Roles ?? new List<string>()));
                }
                else
                {
                    logger.LogError("Failed to create user {Username}: {Errors}", 
                        userSeed.Username, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }

            logger.LogInformation("User seeding completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding users: {Message}", ex.Message);
        }
    }

    private class UserSeedData
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public List<string>? Roles { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}