
namespace ClinicManagement.Domain.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IChronicDiseaseRepository ChronicDiseases { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IMedicalServiceRepository MedicalServices { get; }
    IMedicineRepository Medicines { get; }
    IMedicalSupplyRepository MedicalSupplies { get; }
    IInvoiceRepository Invoices { get; }
    IPaymentRepository Payments { get; }
    IClinicRepository Clinics { get; }
    IClinicBranchRepository ClinicBranches { get; }
    IClinicBranchPhoneNumberRepository ClinicBranchPhoneNumbers { get; }
    IClinicOwnerRepository ClinicOwners { get; }
    IDoctorRepository Doctors { get; }
    IReceptionistRepository Receptionists { get; }
    IStaffInvitationRepository StaffInvitations { get; }
    IPatientRepository Patients { get; }
    IPatientChronicDiseaseRepository PatientChronicDiseases { get; }
    IAppointmentRepository Appointments { get; }
    IAppointmentTypeRepository AppointmentTypes { get; }
    ISpecializationRepository Specializations { get; }
    ISubscriptionPlanRepository SubscriptionPlans { get; }
    IClinicBranchAppointmentPriceRepository ClinicBranchAppointmentPrices { get; }
    IMeasurementAttributeRepository MeasurementAttributes { get; }
    
    // Location repositories for GeoNames snapshot architecture
    ICountryRepository Countries { get; }
    IStateRepository States { get; }
    ICityRepository Cities { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
