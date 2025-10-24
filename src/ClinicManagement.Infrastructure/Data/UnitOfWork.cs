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

    //private IUserRepository? _users;
    private IClinicRepository? _clinics;
    private IPatientRepository? _patients;
    private IAppointmentRepository? _appointments;
    private IDoctorRepository? _doctors;
    private IRepository<Country>? _countries;
    private IRepository<Governorate>? _governorates;
    private IRepository<City>? _cities;
    private IRepository<SubscriptionPlan>? _subscriptionPlans;
    private IRepository<ClinicBranch>? _clinicBranches;
    private IRepository<Specialization>? _specializations;
    private IRepository<DoctorBranch>? _doctorBranches;
    private IRepository<Receptionist>? _receptionists;
    private IRepository<PatientSurgery>? _patientSurgeries;
    private IRepository<Visit>? _visits;
    private IRepository<Medicine>? _medicines;
    private IRepository<PrescriptionMedicine>? _prescriptionMedicines;
    private IRepository<Diagnosis>? _diagnoses;
    private IRepository<ClinicSettings>? _clinicSettings;
    private IRepository<VisitAttributes>? _visitAttributes;
    private IRepository<SpecializationAttribute>? _specializationAttributes;
    private IRepository<VisitAttributeValue>? _visitAttributeValues;
    private IRefreshTokenRepository? _refreshTokens;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }


    //public IUserRepository Users => _users ??= new UserRepository(_context);
    public IUserRepository Users => field ?? new UserRepository(_context);

    public IClinicRepository Clinics => _clinics ??= new ClinicRepository(_context);
    public IPatientRepository Patients => _patients ??= new PatientRepository(_context);
    public IAppointmentRepository Appointments => _appointments ??= new AppointmentRepository(_context);
    public IDoctorRepository Doctors => _doctors ??= new DoctorRepository(_context);
    public IRepository<Country> Countries => _countries ??= new Repository<Country>(_context);
    public IRepository<Governorate> Governorates => _governorates ??= new Repository<Governorate>(_context);
    public IRepository<City> Cities => _cities ??= new Repository<City>(_context);
    public IRepository<SubscriptionPlan> SubscriptionPlans => _subscriptionPlans ??= new Repository<SubscriptionPlan>(_context);
    public IRepository<ClinicBranch> ClinicBranches => _clinicBranches ??= new Repository<ClinicBranch>(_context);
    public IRepository<Specialization> Specializations => _specializations ??= new Repository<Specialization>(_context);
    public IRepository<DoctorBranch> DoctorBranches => _doctorBranches ??= new Repository<DoctorBranch>(_context);
    public IRepository<Receptionist> Receptionists => _receptionists ??= new Repository<Receptionist>(_context);
    public IRepository<PatientSurgery> PatientSurgeries => _patientSurgeries ??= new Repository<PatientSurgery>(_context);
    public IRepository<Visit> Visits => _visits ??= new Repository<Visit>(_context);
    public IRepository<Medicine> Medicines => _medicines ??= new Repository<Medicine>(_context);
    public IRepository<PrescriptionMedicine> PrescriptionMedicines => _prescriptionMedicines ??= new Repository<PrescriptionMedicine>(_context);
    public IRepository<Diagnosis> Diagnoses => _diagnoses ??= new Repository<Diagnosis>(_context);
    public IRepository<ClinicSettings> ClinicSettings => _clinicSettings ??= new Repository<ClinicSettings>(_context);
    public IRepository<VisitAttributes> VisitAttributes => _visitAttributes ??= new Repository<VisitAttributes>(_context);
    public IRepository<SpecializationAttribute> SpecializationAttributes => _specializationAttributes ??= new Repository<SpecializationAttribute>(_context);
    public IRepository<VisitAttributeValue> VisitAttributeValues => _visitAttributeValues ??= new Repository<VisitAttributeValue>(_context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);

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
