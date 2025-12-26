using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Core repositories
    IUserRepository Users { get; }
    IClinicRepository Clinics { get; }
    IRepository<ClinicBranch> ClinicBranches { get; }
    IDoctorRepository Doctors { get; }
    IReceptionistRepository Receptionists { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IPatientRepository Patients { get; }
    IAppointmentRepository Appointments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
