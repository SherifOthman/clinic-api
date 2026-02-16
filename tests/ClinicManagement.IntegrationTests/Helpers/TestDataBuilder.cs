using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.IntegrationTests.Helpers;

/// <summary>
/// Helper class to build test data
/// </summary>
public class TestDataBuilder
{
    private readonly IServiceScope _scope;
    private readonly ApplicationDbContext _db;

    public TestDataBuilder(IServiceProvider serviceProvider)
    {
        _scope = serviceProvider.CreateScope();
        _db = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    }

    public async Task<Clinic> CreateClinicAsync(Guid ownerId, string name = "Test Clinic")
    {
        var subscriptionPlan = _db.SubscriptionPlans.First();
        
        var clinic = new Clinic
        {
            Name = name,
            OwnerUserId = ownerId,
            SubscriptionPlanId = subscriptionPlan.Id
        };

        _db.Clinics.Add(clinic);
        await _db.SaveChangesAsync();
        return clinic;
    }

    public async Task<ClinicBranch> CreateClinicBranchAsync(Guid clinicId, string name = "Main Branch")
    {
        var branch = new ClinicBranch
        {
            ClinicId = clinicId,
            Name = name,
            AddressLine = "123 Test St",
            CountryGeoNameId = 1,
            StateGeoNameId = 1,
            CityGeoNameId = 1
        };

        _db.ClinicBranches.Add(branch);
        await _db.SaveChangesAsync();
        return branch;
    }

    public async Task<Patient> CreatePatientAsync(
        Guid clinicId,
        string fullName = "John Doe",
        bool isMale = true,
        DateTime? dateOfBirth = null)
    {
        var patient = new Patient
        {
            PatientCode = $"PAT-{DateTime.UtcNow.Ticks}",
            ClinicId = clinicId,
            FullName = fullName,
            IsMale = isMale,
            DateOfBirth = dateOfBirth ?? new DateTime(1990, 1, 1)
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
        return patient;
    }

    public async Task<DoctorProfile> CreateDoctorAsync(Guid userId, Guid clinicId, Guid specializationId)
    {
        // Create Staff record first
        var staff = new Staff
        {
            UserId = userId,
            ClinicId = clinicId,
            IsActive = true,
            HireDate = DateTime.UtcNow
        };

        _db.Staff.Add(staff);
        await _db.SaveChangesAsync();

        // Create DoctorProfile
        var doctorProfile = new DoctorProfile
        {
            StaffId = staff.Id,
            SpecializationId = specializationId,
            LicenseNumber = $"LIC-{DateTime.UtcNow.Ticks}"
        };

        _db.DoctorProfiles.Add(doctorProfile);
        await _db.SaveChangesAsync();
        return doctorProfile;
    }

    public async Task<AppointmentType> CreateAppointmentTypeAsync(string nameEn = "Consultation")
    {
        var appointmentType = new AppointmentType
        {
            NameEn = nameEn,
            NameAr = "استشارة",
            IsActive = true,
            DisplayOrder = 1
        };

        _db.AppointmentTypes.Add(appointmentType);
        await _db.SaveChangesAsync();
        return appointmentType;
    }

    public async Task<Medicine> CreateMedicineAsync(
        Guid clinicBranchId,
        string name = "Test Medicine",
        decimal boxPrice = 100m,
        int stripsPerBox = 10,
        int initialStock = 100)
    {
        var medicine = new Medicine
        {
            ClinicBranchId = clinicBranchId,
            Name = name,
            BoxPrice = boxPrice,
            StripsPerBox = stripsPerBox,
            TotalStripsInStock = initialStock,
            IsActive = true
        };

        _db.Medicines.Add(medicine);
        await _db.SaveChangesAsync();
        return medicine;
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
