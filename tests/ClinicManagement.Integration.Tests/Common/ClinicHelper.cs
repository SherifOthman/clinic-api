using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Integration.Tests.Common;

/// <summary>
/// Seeds a fully onboarded clinic owner directly in the DB so integration tests
/// can exercise endpoints that require a clinic context (tenant filter).
/// </summary>
public static class ClinicHelper
{
    /// <summary>
    /// Creates a clinic owner user + clinic + main branch in the DB, logs in,
    /// and returns the bearer token.
    /// </summary>
    public static async Task<string> CreateClinicOwnerAsync(
        IntegrationTestFactory factory,
        HttpClient client)
    {
        var email = $"owner_{Guid.NewGuid():N}@test.com";
        var username = $"owner_{Guid.NewGuid():N}"[..15];

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        var person = new Person { FirstName = "Clinic", LastName = "Owner", Gender = Gender.Male };
        var user = new User
        {
            UserName       = username,
            Email          = email,
            PhoneNumber    = "+966500000001",
            EmailConfirmed = true,
            PersonId       = person.Id,
            Person         = person,
        };
        await userManager.CreateAsync(user, "Test@1234!");
        await userManager.AddToRoleAsync(user, "ClinicOwner");

        // Ensure a subscription plan exists (seeded once per DB)
        var plan = db.Set<SubscriptionPlan>().FirstOrDefault();
        if (plan == null)
        {
            plan = new SubscriptionPlan
            {
                Name = "Test Plan", NameAr = "خطة اختبار",
                Description = "Test", DescriptionAr = "اختبار",
                MonthlyFee = 100, YearlyFee = 1000, SetupFee = 0,
                MaxStaff = 10, MaxBranches = 5, MaxPatientsPerMonth = 100,
                MaxAppointmentsPerMonth = 500, MaxInvoicesPerMonth = 100,
                StorageLimitGB = 10, IsActive = true, DisplayOrder = 1,
                EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
            };
            db.Set<SubscriptionPlan>().Add(plan);
            await db.SaveChangesAsync();
        }

        var clinic = new Clinic
        {
            Name = "Test Clinic",
            OwnerUserId = user.Id,
            SubscriptionPlanId = plan.Id,
            OnboardingCompleted = true,
            IsActive = true,
        };
        db.Set<Clinic>().Add(clinic);

        db.Set<ClinicBranch>().Add(new ClinicBranch
        {
            ClinicId = clinic.Id,
            Name = "Main Branch",
            AddressLine = "123 Test St",
            
            StateGeonameId = 2,
            CityGeonameId = 3,
            IsMainBranch = true,
            IsActive = true,
        });

        var member = new ClinicMember
        {
            PersonId = user.PersonId,
            UserId   = user.Id,
            ClinicId = clinic.Id,
            Role     = ClinicMemberRole.Owner,
            IsActive = true,
        };
        db.Set<ClinicMember>().Add(member);

        await db.SaveChangesAsync();

        // Seed default permissions for the owner member
        var permissionRepo = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
        await permissionRepo.SeedDefaultsAsync(member.Id, ClinicMemberRole.Owner);

        var token = await AuthHelper.LoginAsync(client, email);
        return token!;
    }
}
