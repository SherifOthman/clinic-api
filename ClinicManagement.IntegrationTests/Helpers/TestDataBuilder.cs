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
            Address = "123 Test St",
            CityGeoNameId = 1
        };

        _db.ClinicBranches.Add(branch);
        await _db.SaveChangesAsync();
        return branch;
    }

    public async Task<Patient> CreatePatientAsync(
        Guid clinicId,
        string fullName = "John Doe",
        Gender gender = Gender.Male,
        DateTime? dateOfBirth = null)
    {
        var patient = Patient.Create(
            $"PAT-{DateTime.UtcNow.Ticks}",
            clinicId,
            fullName,
            gender,
            dateOfBirth ?? new DateTime(1990, 1, 1));

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
        return patient;
    }

    public async Task<Doctor> CreateDoctorAsync(Guid userId, Guid clinicId, Guid specializationId)
    {
        var doctor = new Doctor
        {
            UserId = userId,
            ClinicId = clinicId,
            SpecializationId = specializationId,
            LicenseNumber = $"LIC-{DateTime.UtcNow.Ticks}"
        };

        _db.Doctors.Add(doctor);
        await _db.SaveChangesAsync();
        return doctor;
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
        var medicine = Medicine.Create(
            clinicBranchId,
            name,
            boxPrice,
            stripsPerBox,
            initialStock);

        _db.Medicines.Add(medicine);
        await _db.SaveChangesAsync();
        return medicine;
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
