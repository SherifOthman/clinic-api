using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPatientRepository Patients { get; }
    IChronicDiseaseRepository ChronicDiseases { get; }
    IClinicRepository Clinics { get; }
    IRepository<ClinicBranch> ClinicBranches { get; }
    IRepository<ClinicBranchPhoneNumber> ClinicBranchPhoneNumbers { get; }
    ISubscriptionPlanRepository SubscriptionPlans { get; }
    IRateLimitRepository RateLimitEntries { get; }
    IRefreshTokenRepository RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
