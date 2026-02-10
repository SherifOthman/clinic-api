using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClinicManagement.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly Dictionary<Type, object> _repositories;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _repositories = new Dictionary<Type, object>();
    }

    // Specific repositories using C# 14 field keyword
    public IPatientRepository Patients => field ??= new PatientRepository(_context);
    public IChronicDiseaseRepository ChronicDiseases => field ??= new ChronicDiseaseRepository(_context);
    public ISpecializationRepository Specializations => field ??= new SpecializationRepository(_context);
    public IMedicineRepository Medicines => field ??= new MedicineRepository(_context);
    public IMedicalSupplyRepository MedicalSupplies => field ??= new MedicalSupplyRepository(_context);
    public IMedicalServiceRepository MedicalServices => field ??= new MedicalServiceRepository(_context);
    public IAppointmentRepository Appointments => field ??= new AppointmentRepository(_context);
    public IInvoiceRepository Invoices => field ??= new InvoiceRepository(_context);
    public IPaymentRepository Payments => field ??= new PaymentRepository(_context);
    public IMedicalFileRepository MedicalFiles => field ??= new MedicalFileRepository(_context);
    public IRefreshTokenRepository RefreshTokens => field ??= new RefreshTokenRepository(_context, _dateTimeProvider);

    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        
        if (!_repositories.ContainsKey(type))
        {
            _repositories[type] = new BaseRepository<T>(_context);
        }
        
        return (IRepository<T>)_repositories[type];
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}
