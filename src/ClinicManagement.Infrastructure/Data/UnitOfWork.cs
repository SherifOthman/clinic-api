using ClinicManagement.Domain.Repositories;
using ClinicManagement.Infrastructure.Data.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly SqlConnection _connection;
    private readonly IConfiguration _configuration;
    private SqlTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(IConfiguration configuration)
    {
        _configuration = configuration;
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    public IUserRepository Users
    {
        get => field ??= new UserRepository(_connection, _transaction);
        set => field = value;
    }

    public IRefreshTokenRepository RefreshTokens
    {
        get => field ??= new RefreshTokenRepository(_connection, _transaction);
        set => field = value;
    }

    public ISubscriptionPlanRepository SubscriptionPlans
    {
        get => field ??= new SubscriptionPlanRepository(_connection, _transaction);
        set => field = value;
    }

    public IStaffInvitationRepository StaffInvitations
    {
        get => field ??= new StaffInvitationRepository(_connection, _transaction);
        set => field = value;
    }

    public IStaffRepository Staff
    {
        get => field ??= new StaffRepository(_connection, _transaction);
        set => field = value;
    }

    public ISpecializationRepository Specializations
    {
        get => field ??= new SpecializationRepository(_configuration);
        set => field = value;
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = _connection.BeginTransaction();
        
        Users = null!;
        RefreshTokens = null!;
        SubscriptionPlans = null!;
        StaffInvitations = null!;
        Staff = null!;
        // Note: Specializations uses separate connection for caching, not affected by transaction
        
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
        return Task.CompletedTask;
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
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
