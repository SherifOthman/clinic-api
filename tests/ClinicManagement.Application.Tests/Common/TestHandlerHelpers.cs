using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace ClinicManagement.Application.Tests.Common;

public static class TestHandlerHelpers
{
    /// <summary>
    /// Creates a real UnitOfWork backed by an isolated in-memory database.
    /// Use this in ALL tests — never use ApplicationDbContext directly.
    /// </summary>
    public static IUnitOfWork CreateUow()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        var cache   = new MemoryCache(new MemoryCacheOptions());
        return new UnitOfWork(context, cache);
    }

    public static Mock<UserManager<User>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
#pragma warning disable CS8625
        return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625
    }

    // ── Entity factories ──────────────────────────────────────────────────────

    public static User CreateTestUser(string email = "test@test.com", bool emailConfirmed = true) =>
        new() { Email = email, UserName = email, EmailConfirmed = emailConfirmed };

    public static SubscriptionPlan CreateTestSubscriptionPlan(string name = "Test Plan") =>
        new()
        {
            Name = name, NameAr = "خطة اختبار",
            Description = "Test", DescriptionAr = "اختبار",
            MonthlyFee = 100, YearlyFee = 1000, SetupFee = 0,
            MaxStaff = 10, MaxBranches = 5, MaxPatientsPerMonth = 100,
            MaxAppointmentsPerMonth = 500, MaxInvoicesPerMonth = 100,
            StorageLimitGB = 10, IsActive = true, DisplayOrder = 1,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
        };

    public static Specialization CreateTestSpecialization(string nameEn = "General Practice") =>
        new() { NameEn = nameEn, NameAr = "طب عام", IsActive = true };

    public static Clinic CreateTestClinic(Guid? ownerUserId = null, Guid? subscriptionPlanId = null) =>
        new()
        {
            Name = "Test Clinic",
            OwnerUserId = ownerUserId ?? Guid.NewGuid(),
            SubscriptionPlanId = subscriptionPlanId ?? Guid.NewGuid(),
            OnboardingCompleted = true,
            IsActive = true,
        };

    public static ClinicBranch CreateTestBranch(Guid? clinicId = null, bool isMainBranch = true) =>
        new()
        {
            ClinicId = clinicId ?? Guid.NewGuid(),
            Name = "Main Branch",
            AddressLine = "123 Test Street",
            CountryGeoNameId = 1, StateGeoNameId = 2, CityGeoNameId = 3,
            IsMainBranch = isMainBranch, IsActive = true,
        };

    public static Domain.Entities.Staff CreateTestStaff(Guid? userId = null, Guid? clinicId = null) =>
        new() { UserId = userId ?? Guid.NewGuid(), ClinicId = clinicId ?? Guid.NewGuid(), IsActive = true };

    public static DoctorProfile CreateTestDoctorProfile(Guid? staffId = null, Guid? specializationId = null) =>
        new() { StaffId = staffId ?? Guid.NewGuid(), SpecializationId = specializationId ?? Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };
}
