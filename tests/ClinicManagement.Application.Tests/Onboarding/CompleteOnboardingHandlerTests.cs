using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Onboarding.Commands;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClinicManagement.Application.Tests.Onboarding;

public class CompleteOnboardingHandlerTests : IDisposable
{
    private ApplicationDbContext _context = null!;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private CompleteOnboardingHandler _handler = null!;
    private User _testUser = null!;
    private SubscriptionPlan _testPlan = null!;
    private Specialization _testSpecialization = null!;

    public CompleteOnboardingHandlerTests()
    {
        _currentUserMock = new Mock<ICurrentUserService>();
        
        var userStoreMock = new Mock<IUserStore<User>>();
#pragma warning disable CS8625
        _userManagerMock = new Mock<UserManager<User>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        // Create test user
        _testUser = new User
        {
            Email = "owner@test.com",
            UserName = "owner@test.com",
            EmailConfirmed = true
        };
        _context.Users.Add(_testUser);

        // Create test subscription plan
        _testPlan = new SubscriptionPlan
        {
            Name = "Basic Plan",
            NameAr = "خطة أساسية",
            Description = "Basic subscription plan",
            DescriptionAr = "خطة اشتراك أساسية",
            MonthlyFee = 100,
            YearlyFee = 1000,
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
        _context.SubscriptionPlans.Add(_testPlan);

        // Create test specialization
        _testSpecialization = new Specialization
        {
            NameEn = "General Practice",
            NameAr = "طب عام",
            IsActive = true
        };
        _context.Specializations.Add(_testSpecialization);

        _context.SaveChanges();

        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(_testUser.Id);

        _userManagerMock
            .Setup(x => x.GetRolesAsync(_testUser))
            .ReturnsAsync(new List<string> { UserRoles.ClinicOwner });

        _handler = new CompleteOnboardingHandler(
            _context,
            _currentUserMock.Object,
            _userManagerMock.Object);
    }

    [Fact]
    public async Task Handle_OwnerDoctorWithAllOptionalFields_CreatesAllRecordsWithCorrectData()
    {
        // Arrange
        var command = new CompleteOnboarding(
            ClinicName: "Test Clinic",
            SubscriptionPlanId: _testPlan.Id,
            BranchName: "Main Branch",
            AddressLine: "123 Test Street",
            CountryGeoNameId: 1,
            StateGeoNameId: 2,
            CityGeoNameId: 3,
            ProvideMedicalServices: true,
            SpecializationId: _testSpecialization.Id,
            LicenseNumber: "LIC-12345",
            YearsOfExperience: 10
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify Clinic
        var clinic = await _context.Clinics.FirstOrDefaultAsync();
        clinic.Should().NotBeNull();
        clinic!.Name.Should().Be("Test Clinic");
        clinic.OwnerUserId.Should().Be(_testUser.Id);
        clinic.SubscriptionPlanId.Should().Be(_testPlan.Id);
        clinic.OnboardingCompleted.Should().BeTrue();
        clinic.IsActive.Should().BeTrue();

        // Verify ClinicBranch
        var branch = await _context.ClinicBranches.FirstOrDefaultAsync();
        branch.Should().NotBeNull();
        branch!.Name.Should().Be("Main Branch");
        branch.ClinicId.Should().Be(clinic.Id);
        branch.AddressLine.Should().Be("123 Test Street");
        branch.CountryGeoNameId.Should().Be(1);
        branch.StateGeoNameId.Should().Be(2);
        branch.CityGeoNameId.Should().Be(3);
        branch.IsMainBranch.Should().BeTrue();
        branch.IsActive.Should().BeTrue();

        // Verify Staff
        var staff = await _context.Staff.FirstOrDefaultAsync();
        staff.Should().NotBeNull();
        staff!.UserId.Should().Be(_testUser.Id);
        staff.ClinicId.Should().Be(clinic.Id);
        staff.IsActive.Should().BeTrue();
        staff.IsPrimaryClinic.Should().BeTrue();
        staff.Status.Should().Be(StaffStatus.Active);
        staff.HireDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        // Verify DoctorProfile
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync();
        doctorProfile.Should().NotBeNull();
        doctorProfile!.StaffId.Should().Be(staff.Id);
        doctorProfile.SpecializationId.Should().Be(_testSpecialization.Id);
        doctorProfile.LicenseNumber.Should().Be("LIC-12345");
        doctorProfile.YearsOfExperience.Should().Be(10);
        doctorProfile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_OwnerDoctorWithOnlyRequiredFields_CreatesAllRecordsWithNullOptionalFields()
    {
        // Arrange
        var command = new CompleteOnboarding(
            ClinicName: "Test Clinic",
            SubscriptionPlanId: _testPlan.Id,
            BranchName: "Main Branch",
            AddressLine: "123 Test Street",
            CountryGeoNameId: 1,
            StateGeoNameId: 2,
            CityGeoNameId: 3,
            ProvideMedicalServices: true,
            SpecializationId: _testSpecialization.Id,
            LicenseNumber: null,
            YearsOfExperience: null
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify Clinic and Branch exist
        var clinic = await _context.Clinics.FirstOrDefaultAsync();
        clinic.Should().NotBeNull();

        var branch = await _context.ClinicBranches.FirstOrDefaultAsync();
        branch.Should().NotBeNull();

        // Verify Staff exists
        var staff = await _context.Staff.FirstOrDefaultAsync();
        staff.Should().NotBeNull();
        staff!.UserId.Should().Be(_testUser.Id);
        staff.ClinicId.Should().Be(clinic!.Id);

        // Verify DoctorProfile with null optional fields
        var doctorProfile = await _context.DoctorProfiles.FirstOrDefaultAsync();
        doctorProfile.Should().NotBeNull();
        doctorProfile!.StaffId.Should().Be(staff.Id);
        doctorProfile.SpecializationId.Should().Be(_testSpecialization.Id);
        doctorProfile.LicenseNumber.Should().BeNull();
        doctorProfile.YearsOfExperience.Should().BeNull();
    }

    [Fact]
    public async Task Handle_AdministrativeOnlyOnboarding_CreatesOnlyClinicAndBranch()
    {
        // Arrange
        var command = new CompleteOnboarding(
            ClinicName: "Admin Only Clinic",
            SubscriptionPlanId: _testPlan.Id,
            BranchName: "Main Branch",
            AddressLine: "456 Admin Street",
            CountryGeoNameId: 1,
            StateGeoNameId: 2,
            CityGeoNameId: 3,
            ProvideMedicalServices: false,
            SpecializationId: null,
            LicenseNumber: null,
            YearsOfExperience: null
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify Clinic exists
        var clinic = await _context.Clinics.FirstOrDefaultAsync();
        clinic.Should().NotBeNull();
        clinic!.Name.Should().Be("Admin Only Clinic");
        clinic.OwnerUserId.Should().Be(_testUser.Id);

        // Verify ClinicBranch exists
        var branch = await _context.ClinicBranches.FirstOrDefaultAsync();
        branch.Should().NotBeNull();
        branch!.ClinicId.Should().Be(clinic.Id);

        // Verify NO Staff record
        var staffRecords = await _context.Staff.ToListAsync();
        staffRecords.Should().BeEmpty();

        // Verify NO DoctorProfile record
        var doctorProfiles = await _context.DoctorProfiles.ToListAsync();
        doctorProfiles.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UserAlreadyOnboarded_ReturnsFailure()
    {
        // Arrange
        var existingClinic = new Clinic
        {
            Name = "Existing Clinic",
            OwnerUserId = _testUser.Id,
            SubscriptionPlanId = _testPlan.Id,
            OnboardingCompleted = true,
            IsActive = true
        };
        _context.Clinics.Add(existingClinic);
        await _context.SaveChangesAsync();

        var command = new CompleteOnboarding(
            ClinicName: "New Clinic",
            SubscriptionPlanId: _testPlan.Id,
            BranchName: "Main Branch",
            AddressLine: "123 Test Street",
            CountryGeoNameId: 1,
            StateGeoNameId: 2,
            CityGeoNameId: 3,
            ProvideMedicalServices: false,
            SpecializationId: null,
            LicenseNumber: null,
            YearsOfExperience: null
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.ALREADY_ONBOARDED);
        result.ErrorMessage.Should().Be("User has already completed onboarding");

        // Verify only the existing clinic exists
        var clinics = await _context.Clinics.ToListAsync();
        clinics.Should().HaveCount(1);
        clinics[0].Name.Should().Be("Existing Clinic");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var nonExistentUserId = Guid.NewGuid();
        _currentUserMock
            .Setup(x => x.GetRequiredUserId())
            .Returns(nonExistentUserId);

        var command = new CompleteOnboarding(
            ClinicName: "Test Clinic",
            SubscriptionPlanId: _testPlan.Id,
            BranchName: "Main Branch",
            AddressLine: "123 Test Street",
            CountryGeoNameId: 1,
            StateGeoNameId: 2,
            CityGeoNameId: 3,
            ProvideMedicalServices: false,
            SpecializationId: null,
            LicenseNumber: null,
            YearsOfExperience: null
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.USER_NOT_FOUND);
        result.ErrorMessage.Should().Be("User not found");

        // Verify no records were created
        var clinics = await _context.Clinics.ToListAsync();
        clinics.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_InvalidSubscriptionPlan_ReturnsFailure()
    {
        // Arrange
        var invalidPlanId = Guid.NewGuid();
        var command = new CompleteOnboarding(
            ClinicName: "Test Clinic",
            SubscriptionPlanId: invalidPlanId,
            BranchName: "Main Branch",
            AddressLine: "123 Test Street",
            CountryGeoNameId: 1,
            StateGeoNameId: 2,
            CityGeoNameId: 3,
            ProvideMedicalServices: false,
            SpecializationId: null,
            LicenseNumber: null,
            YearsOfExperience: null
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.PLAN_NOT_FOUND);
        result.ErrorMessage.Should().Be("The selected subscription plan does not exist");

        // Verify no records were created
        var clinics = await _context.Clinics.ToListAsync();
        clinics.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UserLacksClinicOwnerRole_ReturnsFailure()
    {
        // Arrange
        _userManagerMock
            .Setup(x => x.GetRolesAsync(_testUser))
            .ReturnsAsync(new List<string> { "SomeOtherRole" });

        var command = new CompleteOnboarding(
            ClinicName: "Test Clinic",
            SubscriptionPlanId: _testPlan.Id,
            BranchName: "Main Branch",
            AddressLine: "123 Test Street",
            CountryGeoNameId: 1,
            StateGeoNameId: 2,
            CityGeoNameId: 3,
            ProvideMedicalServices: false,
            SpecializationId: null,
            LicenseNumber: null,
            YearsOfExperience: null
        );

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.FORBIDDEN);
        result.ErrorMessage.Should().Be("User must be clinic owner");

        // Verify no records were created
        var clinics = await _context.Clinics.ToListAsync();
        clinics.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
