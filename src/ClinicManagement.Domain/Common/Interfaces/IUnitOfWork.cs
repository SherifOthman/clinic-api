namespace ClinicManagement.Domain.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Specific repositories with custom methods
    IPatientRepository Patients { get; }
    IChronicDiseaseRepository ChronicDiseases { get; }
    ISpecializationRepository Specializations { get; }
    IMedicineRepository Medicines { get; }
    IMedicalSupplyRepository MedicalSupplies { get; }
    IMedicalServiceRepository MedicalServices { get; }
    IAppointmentRepository Appointments { get; }
    IInvoiceRepository Invoices { get; }
    IPaymentRepository Payments { get; }

    // Generic repository access for entities without specific repositories
    // Use for: MeasurementAttribute, SubscriptionPlan, PatientChronicDisease, etc.
    IRepository<T> Repository<T>() where T : class;

    // Save changes
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
