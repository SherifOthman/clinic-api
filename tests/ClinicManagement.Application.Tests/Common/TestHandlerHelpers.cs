using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ClinicManagement.Application.Tests.Common;

public static class TestHandlerHelpers
{
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
        return new User
        {
            Email          = email,
            UserName       = email,
            EmailConfirmed = emailConfirmed,
            FullName       = "Test User",
            Gender         = Gender.Male,
        };
    }

    public static Specialization CreateTestSpecialization(string nameEn = "General Practice") =>
        new() { NameEn = nameEn, NameAr = "طب عام", IsActive = true };

    public static Clinic CreateTestClinic(Guid? ownerUserId = null, Guid? subscriptionPlanId = null) =>
        new()
        {
            Name               = "Test Clinic",
            OwnerUserId        = ownerUserId ?? Guid.NewGuid(),
            SubscriptionPlanId = subscriptionPlanId ?? Guid.NewGuid(),
            OnboardingCompleted = true,
            IsActive           = true,
        };

    public static ClinicBranch CreateTestBranch(Guid? clinicId = null, bool isMainBranch = true) =>
        new()
        {
            ClinicId       = clinicId ?? Guid.NewGuid(),
            Name           = "Main Branch",
            AddressLine    = "123 Test Street",
            StateGeonameId = 2,
            CityGeonameId  = 3,
            IsMainBranch   = isMainBranch,
            IsActive       = true,
            IsDeleted      = false,
        };

    public static Patient CreateTestPatient(
        string firstName = "Test", string lastName = "Patient",
        Gender gender = Gender.Male,
        string patientCode = "0001",
        Guid? clinicId = null)
    {
        return new Patient
        {
            ClinicId    = clinicId ?? Guid.NewGuid(),
            PatientCode = patientCode,
            FullName    = $"{firstName} {lastName}".Trim(),
            Gender      = gender,
            DateOfBirth = new DateOnly(1990, 1, 1),
            CreatedAt   = DateTimeOffset.UtcNow,
        };
    }

    public static ClinicMember CreateTestMember(
        Guid? userId = null, Guid? clinicId = null,
        string firstName = "Test", string lastName = "User",
        Gender gender = Gender.Male,
        ClinicMemberRole role = ClinicMemberRole.Doctor)
    {
        return new ClinicMember
        {
            UserId    = userId ?? Guid.NewGuid(),
            ClinicId  = clinicId ?? Guid.NewGuid(),
            Role      = role,
            IsActive  = true,
            IsDeleted = false,
        };
    }

    public static DoctorInfo CreateTestDoctorInfo(Guid clinicMemberId, Guid? specializationId = null) =>
        new() { ClinicMemberId = clinicMemberId, SpecializationId = specializationId };
}
