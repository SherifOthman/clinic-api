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

    // Specific repositories
    private IPatientRepository? _patients;
    private IChronicDiseaseRepository? _chronicDiseases;
    private ISpecializationRepository? _specializations;
    private IMedicineRepository? _medicines;
    private IMedicalSupplyRepository? _medicalSupplies;
    private IMedicalServiceRepository? _medicalServices;
    private IAppointmentRepository? _appointments;
    private IInvoiceRepository? _invoices;
    private IPaymentRepository? _payments;
    private IMedicalFileRepository? _medicalFiles;
    private IRefreshTokenRepository? _refreshTokens;

    public UnitOfWork(ApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _repositories = new Dictionary<Type, object>();
    }

    public IPatientRepository Patients => _patients ??= new PatientRepository(_context);
    public IChronicDiseaseRepository ChronicDiseases => _chronicDiseases ??= new ChronicDiseaseRepository(_context);
    public ISpecializationRepository Specializations => _specializations ??= new SpecializationRepository(_context);
    public IMedicineRepository Medicines => _medicines ??= new MedicineRepository(_context);
    public IMedicalSupplyRepository MedicalSupplies => _medicalSupplies ??= new MedicalSupplyRepository(_context);
    public IMedicalServiceRepository MedicalServices => _medicalServices ??= new MedicalServiceRepository(_context);
    public IAppointmentRepository Appointments => _appointments ??= new AppointmentRepository(_context);
    public IInvoiceRepository Invoices => _invoices ??= new InvoiceRepository(_context);
    public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
    public IMedicalFileRepository MedicalFiles => _medicalFiles ??= new MedicalFileRepository(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context, _dateTimeProvider);

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
