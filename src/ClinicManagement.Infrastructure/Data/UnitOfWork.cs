using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Data.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly SqlConnection _connection;
    private SqlTransaction? _transaction;
    private bool _disposed;

    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;
    private ISubscriptionPlanRepository? _subscriptionPlans;

    public UnitOfWork(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        _connection = new SqlConnection(connectionString);
        _connection.Open();
    }

    public IUserRepository Users => 
        _users ??= new UserRepository(_connection, _transaction);

    public IRefreshTokenRepository RefreshTokens => 
        _refreshTokens ??= new RefreshTokenRepository(_connection, _transaction);

    public ISubscriptionPlanRepository SubscriptionPlans => 
        _subscriptionPlans ??= new SubscriptionPlanRepository(_connection, _transaction);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dapper executes immediately, so this is just for interface compatibility
        return Task.FromResult(0);
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = _connection.BeginTransaction();
        
        // Reset repositories to use new transaction
        _users = new UserRepository(_connection, _transaction);
        _refreshTokens = new RefreshTokenRepository(_connection, _transaction);
        _subscriptionPlans = new SubscriptionPlanRepository(_connection, _transaction);
        
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
