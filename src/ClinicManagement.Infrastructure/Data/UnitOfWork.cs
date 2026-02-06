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
    public IChronicDiseaseRepository ChronicDiseases => field ??= new ChronicDiseaseRepository(_context);
    public IRefreshTokenRepository RefreshTokens => field ??= new RefreshTokenRepository(_context, _currentUserService, _dateTimeProvider);
    public IMedicalServiceRepository MedicalServices => field ??= new MedicalServiceRepository(_context);
    public IMedicineRepository Medicines => field ??= new MedicineRepository(_context);
    public IMedicalSupplyRepository MedicalSupplies => field ??= new MedicalSupplyRepository(_context);
    public IInvoiceRepository Invoices => field ??= new InvoiceRepository(_context);
    public IPaymentRepository Payments => field ??= new PaymentRepository(_context);
    public IClinicBranchRepository ClinicBranches => field ??= new ClinicBranchRepository(_context);
    public IPatientRepository Patients => field ??= new PatientRepository(_context);
    public IPatientChronicDiseaseRepository PatientChronicDiseases => field ??= new PatientChronicDiseaseRepository(_context);
    public IAppointmentRepository Appointments => field ??= new AppointmentRepository(_context);
    public IAppointmentTypeRepository AppointmentTypes => field ??= new AppointmentTypeRepository(_context);
    public ISpecializationRepository Specializations => field ??= new SpecializationRepository(_context);
    public IClinicBranchAppointmentPriceRepository ClinicBranchAppointmentPrices => field ??= new ClinicBranchAppointmentPriceRepository(_context);
    public IMeasurementAttributeRepository MeasurementAttributes => field ??= new MeasurementAttributeRepository(_context);

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
