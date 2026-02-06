
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
    IClinicBranchRepository ClinicBranches { get; }
    IPatientRepository Patients { get; }
    IPatientChronicDiseaseRepository PatientChronicDiseases { get; }
    IAppointmentRepository Appointments { get; }
    IAppointmentTypeRepository AppointmentTypes { get; }
    ISpecializationRepository Specializations { get; }
    IClinicBranchAppointmentPriceRepository ClinicBranchAppointmentPrices { get; }
    IMeasurementAttributeRepository MeasurementAttributes { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
