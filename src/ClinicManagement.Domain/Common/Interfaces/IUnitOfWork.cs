using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IClinicRepository Clinics { get; }
    IPatientRepository Patients { get; }
    IAppointmentRepository Appointments { get; }
    IDoctorRepository Doctors { get; }
    IRepository<Country> Countries { get; }
    IRepository<Governorate> Governorates { get; }
    IRepository<City> Cities { get; }
    IRepository<SubscriptionPlan> SubscriptionPlans { get; }
    IRepository<ClinicBranch> ClinicBranches { get; }
    IRepository<Specialization> Specializations { get; }
    IRepository<DoctorBranch> DoctorBranches { get; }
    IRepository<Receptionist> Receptionists { get; }
    IRepository<PatientSurgery> PatientSurgeries { get; }
    IRepository<Visit> Visits { get; }
    IRepository<Medicine> Medicines { get; }
    IRepository<PrescriptionMedicine> PrescriptionMedicines { get; }
    IRepository<Diagnosis> Diagnoses { get; }
    IRepository<ClinicSettings> ClinicSettings { get; }
    IRepository<VisitAttributes> VisitAttributes { get; }
    IRepository<SpecializationAttribute> SpecializationAttributes { get; }
    IRepository<VisitAttributeValue> VisitAttributeValues { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
