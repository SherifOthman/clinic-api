using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace ClinicManagement.Application.Tests.Common;

public static class TestHandlerHelpers
{
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

    public static User CreateTestUser(string email = "test@test.com", bool emailConfirmed = true)
    {
        var person = new Person { FirstName = "Test", LastName = "User", Gender = Gender.Male };
        return new User
        {
            Email          = email,
            UserName       = email,
            EmailConfirmed = emailConfirmed,
            PersonId       = person.Id,
            Person         = person,
        };
    }

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
            StateGeonameId = 2, CityGeonameId = 3,
            IsMainBranch = isMainBranch, IsActive = true,
        };

    /// <summary>Creates a Person + Patient for testing.</summary>
    public static Patient CreateTestPatient(
        string firstName = "Test", string lastName = "Patient",
        Gender gender = Gender.Male,
        string patientCode = "0000001",
        Guid? clinicId = null)
    {
        var person = new Person
        {
            FirstName   = firstName,
            LastName    = lastName,
            Gender      = gender,
            DateOfBirth = new DateOnly(1990, 1, 1),
        };
        return new Patient
        {
            ClinicId    = clinicId ?? Guid.NewGuid(),
            PatientCode = patientCode,
            PersonId    = person.Id,
            Person      = person,
            CreatedAt   = DateTimeOffset.UtcNow,
        };
    }
    public static (Person person, ClinicMember member) CreateTestMember(
        Guid? userId = null, Guid? clinicId = null,
        string firstName = "Test", string lastName = "User",
        Gender gender = Gender.Male,
        ClinicMemberRole role = ClinicMemberRole.Doctor)
    {
        var person = new Person { FirstName = firstName, LastName = lastName, Gender = gender };
        var member = new ClinicMember
        {
            PersonId = person.Id,
            UserId   = userId ?? Guid.NewGuid(),
            ClinicId = clinicId ?? Guid.NewGuid(),
            Role     = role,
            IsActive = true,
            Person   = person,
        };
        return (person, member);
    }

    /// <summary>Creates a DoctorInfo for a ClinicMember.</summary>
    public static DoctorInfo CreateTestDoctorInfo(Guid clinicMemberId, Guid? specializationId = null) =>
        new() { ClinicMemberId = clinicMemberId, SpecializationId = specializationId };
}
