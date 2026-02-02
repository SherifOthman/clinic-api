using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClinicManagement.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly UserManager<User> _userManager;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users => field ??= new UserRepository(_context, _currentUserService, _userManager);
    public IPatientRepository Patients => field ??= new PatientRepository(_context, _currentUserService);
    public IChronicDiseaseRepository ChronicDiseases => field ??= new ChronicDiseaseRepository(_context);
    public IClinicRepository Clinics => field ??= new ClinicRepository(_context);
    
    // Basic entities using simple base repository
    public IRepository<ClinicBranch> ClinicBranches => field ??= new BaseRepository<ClinicBranch>(_context);
    public IRepository<ClinicBranchPhoneNumber> ClinicBranchPhoneNumbers => field ??= new BaseRepository<ClinicBranchPhoneNumber>(_context);
    
    public ISubscriptionPlanRepository SubscriptionPlans => field ??= new SubscriptionPlanRepository(_context);
    public IRateLimitRepository RateLimitEntries => field ??= new RateLimitRepository(_context);
    public IRefreshTokenRepository RefreshTokens => field ??= new RefreshTokenRepository(_context, _currentUserService, _dateTimeProvider);
    public IUserClinicRepository UserClinics => field ??= new UserClinicRepository(_context);

    public UnitOfWork(ApplicationDbContext context, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider, UserManager<User> userManager)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _userManager = userManager;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
