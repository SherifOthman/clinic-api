namespace ClinicManagement.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    ISubscriptionPlanRepository SubscriptionPlans { get; }
    IStaffInvitationRepository StaffInvitations { get; }
    IStaffRepository Staff { get; }
    ISpecializationRepository Specializations { get; }
    
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
