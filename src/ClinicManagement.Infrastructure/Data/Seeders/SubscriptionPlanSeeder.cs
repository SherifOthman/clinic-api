using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Seeders;

public static class SubscriptionPlanSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var existingPlans = await context.SubscriptionPlans.ToListAsync();
        
        // Define the new plans we want
        var newPlansData = new[]
        {
            new { Name = "Solo Practice", Description = "Perfect for individual doctors and small clinics just getting started", Price = 29.99m, MaxUsers = 3, MaxPatients = 100, MaxClinics = 1, MaxBranches = 1, HasAdvancedReporting = false, HasApiAccess = false, HasPrioritySupport = false, HasCustomBranding = false },
            new { Name = "Growing Clinic", Description = "Ideal for expanding clinics with multiple doctors and staff members", Price = 79.99m, MaxUsers = 15, MaxPatients = 1000, MaxClinics = 1, MaxBranches = 5, HasAdvancedReporting = true, HasApiAccess = false, HasPrioritySupport = true, HasCustomBranding = false },
            new { Name = "Healthcare Network", Description = "Enterprise solution for large healthcare organizations and hospital networks", Price = 199.99m, MaxUsers = -1, MaxPatients = -1, MaxClinics = -1, MaxBranches = -1, HasAdvancedReporting = true, HasApiAccess = true, HasPrioritySupport = true, HasCustomBranding = true }
        };

        // Update existing plans or create new ones
        for (int i = 0; i < newPlansData.Length; i++)
        {
            var planData = newPlansData[i];
            SubscriptionPlan plan;
            
            if (i < existingPlans.Count)
            {
                // Update existing plan
                plan = existingPlans[i];
            }
            else
            {
                // Create new plan
                plan = new SubscriptionPlan();
                context.SubscriptionPlans.Add(plan);
            }
            
            // Update plan properties
            plan.Name = planData.Name;
            plan.Description = planData.Description;
            plan.Price = planData.Price;
            plan.DurationDays = 30;
            plan.MaxUsers = planData.MaxUsers;
            plan.MaxPatients = planData.MaxPatients;
            plan.MaxClinics = planData.MaxClinics;
            plan.MaxBranches = planData.MaxBranches;
            plan.HasAdvancedReporting = planData.HasAdvancedReporting;
            plan.HasApiAccess = planData.HasApiAccess;
            plan.HasPrioritySupport = planData.HasPrioritySupport;
            plan.HasCustomBranding = planData.HasCustomBranding;
            plan.IsActive = true;
        }

        // Remove any extra plans if there are more existing than needed
        if (existingPlans.Count > newPlansData.Length)
        {
            var plansToRemove = existingPlans.Skip(newPlansData.Length).ToList();
            context.SubscriptionPlans.RemoveRange(plansToRemove);
        }

        await context.SaveChangesAsync();
    }
}