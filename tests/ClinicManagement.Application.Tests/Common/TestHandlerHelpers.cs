using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClinicManagement.Application.Tests.Common;

/// <summary>
/// Helper methods for creating test dependencies
/// Reduces boilerplate while keeping tests isolated
/// </summary>
public static class TestHandlerHelpers
{
    /// <summary>
    /// Creates a new in-memory database context with a unique name
    /// Each call returns a fresh, isolated database
    /// </summary>
    public static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Creates a mock UserManager for testing
    /// Reduces repetitive mock setup code
    /// </summary>
    public static Mock<UserManager<User>> CreateMockUserManager()
    {
        var userStoreMock = new Mock<IUserStore<User>>();
        
#pragma warning disable CS8625
        return new Mock<UserManager<User>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625
    }

    /// <summary>
    /// Creates a test user with common defaults
    /// Can be customized per test as needed
    /// </summary>
    public static User CreateTestUser(
        string email = "test@test.com",
        bool emailConfirmed = true)
    {
        return new User
        {
            Email = email,
            UserName = email,
            EmailConfirmed = emailConfirmed
        };
    }

    /// <summary>
    /// Creates a test subscription plan with common defaults
    /// </summary>
    public static SubscriptionPlan CreateTestSubscriptionPlan(
        string name = "Test Plan",
        decimal monthlyFee = 100m)
    {
        return new SubscriptionPlan
        {
            Name = name,
            NameAr = "خطة اختبار",
            Description = "Test subscription plan",
            DescriptionAr = "خطة اشتراك تجريبية",
            MonthlyFee = monthlyFee,
            YearlyFee = monthlyFee * 10,
            SetupFee = 0,
            MaxStaff = 10,
            MaxBranches = 1,
            MaxPatientsPerMonth = 100,
            MaxAppointmentsPerMonth = 500,
            MaxInvoicesPerMonth = 100,
            StorageLimitGB = 10,
            IsActive = true,
            DisplayOrder = 1,
            EffectiveDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test specialization with common defaults
    /// </summary>
    public static Specialization CreateTestSpecialization(
        string nameEn = "General Practice")
    {
        return new Specialization
        {
            NameEn = nameEn,
            NameAr = "طب عام",
            IsActive = true
        };
    }
}
