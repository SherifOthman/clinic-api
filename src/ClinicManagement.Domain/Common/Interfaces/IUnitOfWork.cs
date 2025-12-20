using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Essential repositories for Auth and Staff Inviting only
    IUserRepository Users { get; }
    IClinicRepository Clinics { get; }
    IDoctorRepository Doctors { get; }
    IRepository<Receptionist> Receptionists { get; }
    IRepository<Specialization> Specializations { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
