using ClinicManagement.Domain.Repositories;
using ClinicManagement.Infrastructure.Persistence.Data.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Infrastructure.Persistence.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly SqlConnection _connection;
    private SqlTransaction? _transaction;
    private bool _disposed;

    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;
    private ISubscriptionPlanRepository? _subscriptionPlans;
    private IStaffInvitationRepository? _staffInvitations;
    private IStaffRepository? _staff;
    private IDoctorProfileRepository? _doctorProfiles;
    private ISpecializationRepository? _specializations;
    private IClinicRepository? _clinics;
    private IClinicBranchRepository? _clinicBranches;
    private IUserRoleHistoryRepository? _userRoleHistory;
    private IClinicSubscriptionRepository? _clinicSubscriptions;
    private IClinicUsageMetricsRepository? _clinicUsageMetrics;
    private INotificationRepository? _notifications;
    private IEmailQueueRepository? _emailQueue;
    private IDoctorSpecializationRepository? _doctorSpecializations;
    private IStaffBranchRepository? _staffBranches;

    public UnitOfWork(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    public IUserRepository Users => _users ??= new UserRepository(_connection, _transaction);

    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_connection, _transaction);

    public ISubscriptionPlanRepository SubscriptionPlans => _subscriptionPlans ??= new SubscriptionPlanRepository(_connection, _transaction);

    public IStaffInvitationRepository StaffInvitations => _staffInvitations ??= new StaffInvitationRepository(_connection, _transaction);

    public IStaffRepository Staff => _staff ??= new StaffRepository(_connection, _transaction);

    public IDoctorProfileRepository DoctorProfiles => _doctorProfiles ??= new DoctorProfileRepository(_connection, _transaction);

    public ISpecializationRepository Specializations => _specializations ??= new SpecializationRepository(_connection, _transaction);

    public IClinicRepository Clinics => _clinics ??= new ClinicRepository(_connection, _transaction);

    public IClinicBranchRepository ClinicBranches => _clinicBranches ??= new ClinicBranchRepository(_connection, _transaction);

    public IUserRoleHistoryRepository UserRoleHistory => _userRoleHistory ??= new UserRoleHistoryRepository(_connection, _transaction);

    public IClinicSubscriptionRepository ClinicSubscriptions => _clinicSubscriptions ??= new ClinicSubscriptionRepository(_connection, _transaction);

    public IClinicUsageMetricsRepository ClinicUsageMetrics => _clinicUsageMetrics ??= new ClinicUsageMetricsRepository(_connection, _transaction);

    public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_connection, _transaction);

    public IEmailQueueRepository EmailQueue => _emailQueue ??= new EmailQueueRepository(_connection, _transaction);

    public IDoctorSpecializationRepository DoctorSpecializations => _doctorSpecializations ??= new DoctorSpecializationRepository(_connection, _transaction);

    public IStaffBranchRepository StaffBranches => _staffBranches ??= new StaffBranchRepository(_connection, _transaction);

    private void ResetRepositories()
    {
        _users = null;
        _refreshTokens = null;
        _subscriptionPlans = null;
        _staffInvitations = null;
        _staff = null;
        _doctorProfiles = null;
        _specializations = null;
        _clinics = null;
        _clinicBranches = null;
        _userRoleHistory = null;
        _clinicSubscriptions = null;
        _clinicUsageMetrics = null;
        _notifications = null;
        _emailQueue = null;
        _doctorSpecializations = null;
        _staffBranches = null;
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Transaction already started");
        }

        _transaction = _connection.BeginTransaction();
        ResetRepositories();
        
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to commit");
        }

        try
        {
            _transaction.Commit();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
            ResetRepositories();
        }

        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction to rollback");
        }

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            _transaction.Dispose();
            _transaction = null;
            ResetRepositories();
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
        }
        _disposed = true;
    }
}
