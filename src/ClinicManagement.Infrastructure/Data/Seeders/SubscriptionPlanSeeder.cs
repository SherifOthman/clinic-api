using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Seeders;

public static class SubscriptionPlanSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.SubscriptionPlans.AnyAsync())
            return;

        var plans = new[]
        {
            new SubscriptionPlan
            {
                Name = "Starter",
                Description = "Perfect for solo practitioners and small clinics starting their digital journey",
                Price = 49.99m,
                DurationDays = 30,
                MaxUsers = 2,
                MaxPatients = 50,
                IsActive = true
            },
            new SubscriptionPlan
            {
                Name = "Professional",
                Description = "Ideal for growing clinics with multiple doctors and staff members",
                Price = 99.99m,
                DurationDays = 30,
                MaxUsers = 10,
                MaxPatients = 500,
                IsActive = true
            },
            new SubscriptionPlan
            {
                Name = "Enterprise",
                Description = "For large medical centers and hospital networks with advanced needs",
                Price = 199.99m,
                DurationDays = 30,
                MaxUsers = 50,
                MaxPatients = 2000,
                IsActive = true
            },
            new SubscriptionPlan
            {
                Name = "Premium",
                Description = "Unlimited solution for large healthcare organizations and multi-location clinics",
                Price = 399.99m,
                DurationDays = 30,
                MaxUsers = -1, // Unlimited
                MaxPatients = -1, // Unlimited
                IsActive = true
            }
        };

        context.SubscriptionPlans.AddRange(plans);
        await context.SaveChangesAsync();
    }
}