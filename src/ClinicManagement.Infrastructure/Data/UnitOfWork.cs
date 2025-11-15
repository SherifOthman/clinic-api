using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using System.Xml;

namespace ClinicManagement.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users => field ?? new UserRepository(_context);
    public IClinicRepository Clinics => field ??= new ClinicRepository(_context);
    public IPatientRepository Patients => field ??= new PatientRepository(_context);
    public IAppointmentRepository Appointments => field ??= new AppointmentRepository(_context);
    public IDoctorRepository Doctors => field ??= new DoctorRepository(_context);
    public IRepository<Country> Countries => field ??= new Repository<Country>(_context);
    public IRepository<Governorate> Governorates => field ??= new Repository<Governorate>(_context);
    public IRepository<City> Cities => field ??= new Repository<City>(_context);
    public IRepository<SubscriptionPlan> SubscriptionPlans => field ??= new Repository<SubscriptionPlan>(_context);
    public IRepository<ClinicBranch> ClinicBranches => field ??= new Repository<ClinicBranch>(_context);
    public IRepository<Specialization> Specializations => field ??= new Repository<Specialization>(_context);
    public IRepository<DoctorBranch> DoctorBranches => field ??= new Repository<DoctorBranch>(_context);
    public IRepository<Receptionist> Receptionists => field ??= new Repository<Receptionist>(_context);
    public IRepository<PatientSurgery> PatientSurgeries => field ??= new Repository<PatientSurgery>(_context);
    public IRepository<Visit> Visits => field ??= new Repository<Visit>(_context);
    public IMedicineRepository Medicines => field ??= new MedicineRepository(_context);
    public IRepository<PrescriptionMedicine> PrescriptionMedicines => field ??= new Repository<PrescriptionMedicine>(_context);
    public IRepository<Diagnosis> Diagnoses => field ??= new Repository<Diagnosis>(_context);
    public IRepository<ClinicSettings> ClinicSettings => field ??= new Repository<ClinicSettings>(_context);
    public IRepository<VisitAttributes> VisitAttributes => field ??= new Repository<VisitAttributes>(_context);
    public IRepository<SpecializationAttribute> SpecializationAttributes => field ??= new Repository<SpecializationAttribute>(_context);
    public IRepository<VisitAttributeValue> VisitAttributeValues => field ??= new Repository<VisitAttributeValue>(_context);
    public IRefreshTokenRepository RefreshTokens => field ??= new RefreshTokenRepository(_context);
    public IReviewRepository Reviews => field ??= new ReviewRepository(_context);

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
