namespace ClinicManagement.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    ISubscriptionPlanRepository SubscriptionPlans { get; }
    IStaffInvitationRepository StaffInvitations { get; }
    IStaffRepository Staff { get; }
    IDoctorProfileRepository DoctorProfiles { get; }
    ISpecializationRepository Specializations { get; }
    IClinicRepository Clinics { get; }
    IClinicBranchRepository ClinicBranches { get; }
    IUserRoleHistoryRepository UserRoleHistory { get; }
    IClinicSubscriptionRepository ClinicSubscriptions { get; }
    IClinicUsageMetricsRepository ClinicUsageMetrics { get; }
    INotificationRepository Notifications { get; }
    IEmailQueueRepository EmailQueue { get; }
    IDoctorSpecializationRepository DoctorSpecializations { get; }
    IStaffBranchRepository StaffBranches { get; }
    
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
