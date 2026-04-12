using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Persistence;

/// <summary>
/// Concrete Unit of Work.
/// All repos share the same DbContext — one transaction per request.
/// Uses C# 13 field keyword for lazy auto-property backing fields.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public UnitOfWork(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache   = cache;
    }

    public IPatientRepository            Patients          => field ??= new PatientRepository(_context);
    public IStaffRepository              Staff             => field ??= new StaffRepository(_context);
    public IInvitationRepository         Invitations       => field ??= new InvitationRepository(_context);
    public IClinicRepository             Clinics           => field ??= new ClinicRepository(_context);
    public IBranchRepository             Branches          => field ??= new BranchRepository(_context);
    public IUserRepository               Users             => field ??= new UserRepository(_context);
    public IAuditLogRepository           AuditLogs         => field ??= new AuditLogRepository(_context);
    public IDoctorProfileRepository      DoctorProfiles    => field ??= new DoctorProfileRepository(_context);
    public IWorkingDaysRepository        WorkingDays       => field ??= new WorkingDaysRepository(_context);
    public IReferenceRepository          Reference         => field ??= new ReferenceRepository(_context, _cache);
    public IClinicSubscriptionRepository ClinicSubscriptions => field ??= new ClinicSubscriptionRepository(_context);

    // Generic repos for reference/lookup entities — used for seeding in tests
    public IRepository<ChronicDisease>   ChronicDiseases   => field ??= new Repository<ChronicDisease>(_context);
    public IRepository<Specialization>   Specializations   => field ??= new Repository<Specialization>(_context);
    public IRepository<SubscriptionPlan> SubscriptionPlans => field ??= new Repository<SubscriptionPlan>(_context);
    public IUserSeedRepository           UserEntities      => field ??= new UserSeedRepository(_context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
