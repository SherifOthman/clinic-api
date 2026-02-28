using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Onboarding.Commands;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Infrastructure.Persistence;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ClinicManagement.Application.Tests.Onboarding;

public class CompleteOnboardingPropertyTests : IDisposable
{
    private ApplicationDbContext _context = null!;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private CompleteOnboardingHandler _handler = null!;
    private User _testUser = null!;
    private SubscriptionPlan _testPlan = null!;
    private Specialization _testSpecialization = null!;

    public CompleteOnboardingPropertyTests()
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

    // Feature: owner-doctor-onboarding, Property 1
    [Property(MaxTest = 100)]
    public void TransactionAtomicity_RollsBackOnFailure()
    {
        Prop.ForAll(
            ValidOwnerDoctorOnboardingGenerator(),
            (command) =>
            {
                // Reinitialize database for each test iteration
                InitializeDatabase();

                // Test 1: Simulate failure by marking user as already onboarded
                // This will cause the handler to return a failure result
                var existingClinic = new Clinic
                {
                    Name = "Existing Clinic",
                    OwnerUserId = _testUser.Id,
                    SubscriptionPlanId = _testPlan.Id,
                    OnboardingCompleted = true,
                    IsActive = true
                };
                _context.Clinics.Add(existingClinic);
                _context.SaveChanges();

                var commandForAlreadyOnboarded = command with
                {
                    SpecializationId = _testSpecialization.Id,
                    SubscriptionPlanId = _testPlan.Id
                };

                var failureTask = _handler.Handle(commandForAlreadyOnboarded, default);
                failureTask.Wait();
                var failureResult = failureTask.Result;

                failureResult.IsSuccess.Should().BeFalse("command should fail when user already onboarded");

                // Verify only the existing clinic exists, no new records created
                var clinics = _context.Clinics.ToList();
                var branches = _context.ClinicBranches.ToList();
                var staff = _context.Staff.ToList();
                var doctorProfiles = _context.DoctorProfiles.ToList();

                clinics.Should().HaveCount(1, "only the existing clinic should exist");
                branches.Should().BeEmpty("no branches should be created for failed onboarding");
                staff.Should().BeEmpty("no staff should be created for failed onboarding");
                doctorProfiles.Should().BeEmpty("no doctor profiles should be created for failed onboarding");

                // Test 2: Verify successful case creates all records
                InitializeDatabase();

                var validCommand = command with
                {
                    SpecializationId = _testSpecialization.Id,
                    SubscriptionPlanId = _testPlan.Id
                };

                var successTask = _handler.Handle(validCommand, default);
                successTask.Wait();
                var result = successTask.Result;

                result.IsSuccess.Should().BeTrue("valid command should succeed");

                var clinicsAfterSuccess = _context.Clinics.ToList();
                var branchesAfterSuccess = _context.ClinicBranches.ToList();
                var staffAfterSuccess = _context.Staff.ToList();
                var doctorProfilesAfterSuccess = _context.DoctorProfiles.ToList();

                clinicsAfterSuccess.Should().HaveCount(1, "one Clinic should be created");
                branchesAfterSuccess.Should().HaveCount(1, "one ClinicBranch should be created");
                staffAfterSuccess.Should().HaveCount(1, "one Staff record should be created for owner-doctor");
                doctorProfilesAfterSuccess.Should().HaveCount(1, "one DoctorProfile should be created for owner-doctor");

                // Verify relationships and data integrity
                var clinic = clinicsAfterSuccess.First();
                var branch = branchesAfterSuccess.First();
                var staffRecord = staffAfterSuccess.First();
                var doctorProfile = doctorProfilesAfterSuccess.First();

                clinic.OwnerUserId.Should().Be(_testUser.Id);
                clinic.OnboardingCompleted.Should().BeTrue();
                clinic.IsActive.Should().BeTrue();
                
                branch.ClinicId.Should().Be(clinic.Id);
                branch.IsMainBranch.Should().BeTrue();
                branch.IsActive.Should().BeTrue();
                
                staffRecord.UserId.Should().Be(_testUser.Id);
                staffRecord.ClinicId.Should().Be(clinic.Id);
                staffRecord.IsPrimaryClinic.Should().BeTrue();
                staffRecord.Status.Should().Be(StaffStatus.Active);
                staffRecord.IsActive.Should().BeTrue();
                
                doctorProfile.StaffId.Should().Be(staffRecord.Id);
                doctorProfile.SpecializationId.Should().Be(_testSpecialization.Id);
                doctorProfile.LicenseNumber.Should().Be(command.LicenseNumber);
                doctorProfile.YearsOfExperience.Should().Be(command.YearsOfExperience);

                return true;
            }).QuickCheckThrowOnFailure();
    }

    private static Arbitrary<CompleteOnboarding> ValidOwnerDoctorOnboardingGenerator()
    {
        return Arb.From(
            from clinicName in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 100)))
            from branchName in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 100)))
            from addressLine in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 200)))
            from countryId in Gen.Choose(1, 1000)
            from stateId in Gen.Choose(1, 1000)
            from cityId in Gen.Choose(1, 1000)
            from licenseNumber in Gen.Elements<string?>(null, "LIC123", "ABC-456-789", "12345")
            from yearsExp in Gen.Choose(0, 70).Select(x => (int?)x)
            select new CompleteOnboarding(
                clinicName,
                Guid.NewGuid(), // Will be replaced with valid plan ID in test
                branchName,
                addressLine,
                countryId,
                stateId,
                cityId,
                true, // provideMedicalServices
                Guid.NewGuid(), // Will be replaced with valid specialization ID in test
                licenseNumber,
                yearsExp
            )
        );
    }

    // Feature: owner-doctor-onboarding, Property 2
    // **Validates: Requirements 2.1**
    [Property(MaxTest = 100)]
    public void StaffRecordCreation_ForOwnerDoctors()
    {
        Prop.ForAll(
            ValidOwnerDoctorOnboardingGenerator(),
            (command) =>
            {
                // Reinitialize database for each test iteration
                InitializeDatabase();

                var validCommand = command with
                {
                    SpecializationId = _testSpecialization.Id,
                    SubscriptionPlanId = _testPlan.Id
                };

                var task = _handler.Handle(validCommand, default);
                task.Wait();
                var result = task.Result;

                result.IsSuccess.Should().BeTrue("valid owner-doctor onboarding should succeed");

                // Verify Staff record exists
                var staffRecords = _context.Staff.ToList();
                staffRecords.Should().HaveCount(1, "exactly one Staff record should be created for owner-doctor");

                var staff = staffRecords.First();
                
                // Verify Staff record has correct UserId
                staff.UserId.Should().Be(_testUser.Id, "Staff record should link to the owner's UserId");
                
                // Verify Staff record has correct ClinicId
                var clinic = _context.Clinics.First();
                staff.ClinicId.Should().Be(clinic.Id, "Staff record should link to the newly created ClinicId");
                
                // Verify Staff record is immediately queryable
                var queriedStaff = _context.Staff
                    .Where(s => s.UserId == _testUser.Id && s.ClinicId == clinic.Id)
                    .FirstOrDefault();
                
                queriedStaff.Should().NotBeNull("Staff record should be queryable immediately after onboarding");
                queriedStaff!.Id.Should().Be(staff.Id, "queried Staff record should match the created record");
                
                // Verify additional Staff properties
                staff.IsPrimaryClinic.Should().BeTrue("owner-doctor's clinic should be marked as primary");
                staff.Status.Should().Be(StaffStatus.Active, "owner-doctor should have Active status");
                staff.IsActive.Should().BeTrue("owner-doctor should be active");

                return true;
            }).QuickCheckThrowOnFailure();
    }

    // Feature: owner-doctor-onboarding, Property 3
    // **Validates: Requirements 3.1**
    [Property(MaxTest = 100)]
    public void DoctorProfileCreation_ForOwnerDoctors()
    {
        Prop.ForAll(
            ValidOwnerDoctorOnboardingGenerator(),
            (command) =>
            {
                // Reinitialize database for each test iteration
                InitializeDatabase();

                var validCommand = command with
                {
                    SpecializationId = _testSpecialization.Id,
                    SubscriptionPlanId = _testPlan.Id
                };

                var task = _handler.Handle(validCommand, default);
                task.Wait();
                var result = task.Result;

                result.IsSuccess.Should().BeTrue("valid owner-doctor onboarding should succeed");

                // Verify DoctorProfile record exists
                var doctorProfiles = _context.DoctorProfiles.ToList();
                doctorProfiles.Should().HaveCount(1, "exactly one DoctorProfile should be created for owner-doctor");

                var doctorProfile = doctorProfiles.First();
                
                // Verify DoctorProfile is linked to Staff record
                var staff = _context.Staff.First();
                doctorProfile.StaffId.Should().Be(staff.Id, "DoctorProfile should be linked to the Staff record");
                
                // Verify DoctorProfile has correct SpecializationId
                doctorProfile.SpecializationId.Should().Be(_testSpecialization.Id, "DoctorProfile should have the correct SpecializationId");
                
                // Verify DoctorProfile has correct LicenseNumber
                doctorProfile.LicenseNumber.Should().Be(command.LicenseNumber, "DoctorProfile should have the correct LicenseNumber");
                
                // Verify DoctorProfile has correct YearsOfExperience
                doctorProfile.YearsOfExperience.Should().Be(command.YearsOfExperience, "DoctorProfile should have the correct YearsOfExperience");
                
                // Verify DoctorProfile is immediately queryable
                var queriedProfile = _context.DoctorProfiles
                    .Where(dp => dp.StaffId == staff.Id)
                    .FirstOrDefault();
                
                queriedProfile.Should().NotBeNull("DoctorProfile should be queryable immediately after onboarding");
                queriedProfile!.Id.Should().Be(doctorProfile.Id, "queried DoctorProfile should match the created record");
                
                // Verify CreatedAt timestamp is set
                doctorProfile.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1), "CreatedAt should be set to current time");

                return true;
            }).QuickCheckThrowOnFailure();
    }

    // Feature: owner-doctor-onboarding, Property 4
    // **Validates: Requirements 4.1, 4.2**
    [Property(MaxTest = 100)]
    public void NoStaffOrDoctorProfile_ForAdministrativeOnlyOwners()
    {
        Prop.ForAll(
            ValidAdministrativeOnlyOnboardingGenerator(),
            (command) =>
            {
                // Reinitialize database for each test iteration
                InitializeDatabase();

                var validCommand = command with
                {
                    SubscriptionPlanId = _testPlan.Id
                };

                var task = _handler.Handle(validCommand, default);
                task.Wait();
                var result = task.Result;

                result.IsSuccess.Should().BeTrue("valid administrative-only onboarding should succeed");

                // Verify Clinic record exists
                var clinics = _context.Clinics.ToList();
                clinics.Should().HaveCount(1, "exactly one Clinic should be created");

                var clinic = clinics.First();
                clinic.OwnerUserId.Should().Be(_testUser.Id, "Clinic should be owned by the test user");
                clinic.OnboardingCompleted.Should().BeTrue("onboarding should be marked as completed");
                clinic.IsActive.Should().BeTrue("Clinic should be active");

                // Verify ClinicBranch record exists
                var branches = _context.ClinicBranches.ToList();
                branches.Should().HaveCount(1, "exactly one ClinicBranch should be created");

                var branch = branches.First();
                branch.ClinicId.Should().Be(clinic.Id, "ClinicBranch should be linked to the Clinic");
                branch.IsMainBranch.Should().BeTrue("first branch should be marked as main branch");
                branch.IsActive.Should().BeTrue("ClinicBranch should be active");

                // Verify NO Staff record exists
                var staffRecords = _context.Staff.ToList();
                staffRecords.Should().BeEmpty("no Staff record should be created for administrative-only owners");

                // Verify NO DoctorProfile record exists
                var doctorProfiles = _context.DoctorProfiles.ToList();
                doctorProfiles.Should().BeEmpty("no DoctorProfile should be created for administrative-only owners");

                // Verify the command had provideMedicalServices = false
                command.ProvideMedicalServices.Should().BeFalse("this test is for administrative-only path");

                return true;
            }).QuickCheckThrowOnFailure();
    }

    private static Arbitrary<CompleteOnboarding> ValidAdministrativeOnlyOnboardingGenerator()
    {
        return Arb.From(
            from clinicName in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 100)))
            from branchName in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 100)))
            from addressLine in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 200)))
            from countryId in Gen.Choose(1, 1000)
            from stateId in Gen.Choose(1, 1000)
            from cityId in Gen.Choose(1, 1000)
            select new CompleteOnboarding(
                clinicName,
                Guid.NewGuid(), // Will be replaced with valid plan ID in test
                branchName,
                addressLine,
                countryId,
                stateId,
                cityId,
                false, // provideMedicalServices = false for administrative-only
                null, // SpecializationId not needed
                null, // LicenseNumber not needed
                null  // YearsOfExperience not needed
            )
        );
    }

    // Feature: owner-doctor-onboarding, Property 7
    // **Validates: Requirements 8.1**
    [Property(MaxTest = 100)]
    public void RequiredFieldValidation_ForDoctorProfiles()
    {
        Prop.ForAll(
            OwnerDoctorOnboardingWithMissingSpecializationGenerator(),
            (command) =>
            {
                // Reinitialize database for each test iteration
                InitializeDatabase();

                // Create validator instance
                var validator = new CompleteOnboardingValidator();

                // Test with missing SpecializationId (should fail validation)
                var commandWithMissingSpecialization = command with
                {
                    SubscriptionPlanId = _testPlan.Id,
                    SpecializationId = null // Omit SpecializationId
                };

                var validationResult = validator.Validate(commandWithMissingSpecialization);

                validationResult.IsValid.Should().BeFalse("validation should fail when SpecializationId is missing for owner-doctor");
                validationResult.Errors.Should().Contain(e => e.PropertyName == "SpecializationId", 
                    "validation error should be for SpecializationId field");
                validationResult.Errors.Should().Contain(e => e.ErrorMessage.Contains("Specialization is required"), 
                    "error message should indicate specialization is required");

                // Test with valid SpecializationId (should pass validation)
                var commandWithValidSpecialization = command with
                {
                    SubscriptionPlanId = _testPlan.Id,
                    SpecializationId = _testSpecialization.Id
                };

                var validValidationResult = validator.Validate(commandWithValidSpecialization);

                validValidationResult.IsValid.Should().BeTrue("validation should pass when all required fields are present");

                return true;
            }).QuickCheckThrowOnFailure();
    }

    private static Arbitrary<CompleteOnboarding> OwnerDoctorOnboardingWithMissingSpecializationGenerator()
    {
        return Arb.From(
            from clinicName in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 100))).Where(s => !string.IsNullOrWhiteSpace(s))
            from branchName in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 100))).Where(s => !string.IsNullOrWhiteSpace(s))
            from addressLine in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 200))).Where(s => !string.IsNullOrWhiteSpace(s))
            from countryId in Gen.Choose(1, 1000)
            from stateId in Gen.Choose(1, 1000)
            from cityId in Gen.Choose(1, 1000)
            from licenseNumber in Gen.Elements<string?>(null, "LIC123", "ABC-456-789", "12345")
            from yearsExp in Gen.Choose(0, 70).Select(x => (int?)x)
            select new CompleteOnboarding(
                clinicName,
                Guid.NewGuid(), // Will be replaced with valid plan ID in test
                branchName,
                addressLine,
                countryId,
                stateId,
                cityId,
                true, // provideMedicalServices
                null, // SpecializationId intentionally null for testing validation
                licenseNumber,
                yearsExp
            )
        );
    }

    // Feature: owner-doctor-onboarding, Property 8
    // **Validates: Requirements 8.2**
    [Property(MaxTest = 100)]
    public void MedicalCredentialFormatValidation()
    {
        Prop.ForAll(
            InvalidMedicalCredentialGenerator(),
            (command) =>
            {
                // Create validator instance
                var validator = new CompleteOnboardingValidator();

                // Test with invalid credentials
                var validationResult = validator.Validate(command);

                validationResult.IsValid.Should().BeFalse("validation should fail when medical credential format is invalid");

                // Check if LicenseNumber is invalid (>50 chars)
                if (command.LicenseNumber != null && command.LicenseNumber.Length > 50)
                {
                    validationResult.Errors.Should().Contain(e => e.PropertyName == "LicenseNumber",
                        "validation error should be for LicenseNumber field when it exceeds 50 characters");
                }

                // Check if YearsOfExperience is invalid (<0 or >70)
                if (command.YearsOfExperience.HasValue && 
                    (command.YearsOfExperience.Value < 0 || command.YearsOfExperience.Value > 70))
                {
                    validationResult.Errors.Should().Contain(e => e.PropertyName == "YearsOfExperience",
                        "validation error should be for YearsOfExperience field when it's out of range");
                }

                return true;
            }).QuickCheckThrowOnFailure();
    }

    private static Arbitrary<CompleteOnboarding> InvalidMedicalCredentialGenerator()
    {
        var alphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
        
        return Arb.From(
            from clinicName in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 100))).Where(s => !string.IsNullOrWhiteSpace(s))
            from branchName in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 100))).Where(s => !string.IsNullOrWhiteSpace(s))
            from addressLine in Arb.Generate<NonEmptyString>().Select(s => s.Get.Substring(0, Math.Min(s.Get.Length, 200))).Where(s => !string.IsNullOrWhiteSpace(s))
            from countryId in Gen.Choose(1, 1000)
            from stateId in Gen.Choose(1, 1000)
            from cityId in Gen.Choose(1, 1000)
            from invalidCredential in Gen.OneOf(
                // Generate invalid LicenseNumber (>50 chars)
                Gen.Choose(51, 100).SelectMany(len => 
                    Gen.ArrayOf(len, Gen.Elements(alphanumericChars))
                        .Select(chars => new string(chars))
                        .Select(license => (license: (string?)license, years: (int?)Gen.Choose(0, 70).Sample(0, 1).First()))),
                // Generate invalid YearsOfExperience (<0)
                Gen.Choose(-50, -1).Select(years => 
                    (license: (string?)null, years: (int?)years)),
                // Generate invalid YearsOfExperience (>70)
                Gen.Choose(71, 150).Select(years => 
                    (license: (string?)null, years: (int?)years)),
                // Generate both invalid
                Gen.Choose(51, 100).SelectMany(len => 
                    Gen.ArrayOf(len, Gen.Elements(alphanumericChars))
                        .Select(chars => new string(chars)))
                    .SelectMany(license => 
                        Gen.OneOf(Gen.Choose(-50, -1), Gen.Choose(71, 150))
                            .Select(years => (license: (string?)license, years: (int?)years)))
            )
            select new CompleteOnboarding(
                clinicName,
                Guid.NewGuid(), // Will be replaced with valid plan ID in test
                branchName,
                addressLine,
                countryId,
                stateId,
                cityId,
                true, // provideMedicalServices
                Guid.NewGuid(), // Valid specialization ID
                invalidCredential.license,
                invalidCredential.years
            )
        );
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
