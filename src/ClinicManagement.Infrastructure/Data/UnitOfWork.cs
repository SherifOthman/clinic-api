using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClinicManagement.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    // Core repositories
    public IUserRepository Users => field ??= new UserRepository(_context);
    public IClinicRepository Clinics => field ??= new ClinicRepository(_context);
    public IRepository<ClinicBranch> ClinicBranches => field ??= new Repository<ClinicBranch>(_context);
    public IDoctorRepository Doctors => field ??= new DoctorRepository(_context);
    public IReceptionistRepository Receptionists => field ??= new ReceptionistRepository(_context);
    public IRefreshTokenRepository RefreshTokens => field ??= new RefreshTokenRepository(_context);
    public IPatientRepository Patients => field ??= new PatientRepository(_context);
    public IAppointmentRepository Appointments => field ??= new AppointmentRepository(_context);

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
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
