using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Data;

/// <summary>
/// Single entry point for all data access in handlers.
/// Handlers inject ONLY IUnitOfWork — no direct DbContext access.
///
/// Usage pattern:
///   var patients = await _uow.Patients.GetPatientListAsync(...);
///   _uow.Patients.AddAsync(patient);
///   await _uow.SaveChangesAsync(ct);
/// </summary>
public interface IUnitOfWork
{
    IPatientRepository Patients { get; }

    // ── Old staff repositories (kept during migration) ────────────────────────
    IStaffRepository Staff { get; }
    IDoctorProfileRepository DoctorProfiles { get; }
    IWorkingDaysRepository WorkingDays { get; }
    IDoctorVisitTypeRepository DoctorVisitTypes { get; }

    // ── New repositories (use these going forward) ────────────────────────────
    IClinicMemberRepository Members { get; }
    IDoctorInfoRepository DoctorInfos { get; }
    IDoctorScheduleRepository DoctorSchedules { get; }

    IInvitationRepository Invitations { get; }
    IClinicRepository Clinics { get; }
    IBranchRepository Branches { get; }
    IUserRepository Users { get; }
    IAuditLogRepository AuditLogs { get; }
    IReferenceRepository Reference { get; }
    IClinicSubscriptionRepository ClinicSubscriptions { get; }
    IGeoLocationRepository GeoLocations { get; }

    // Generic repos for reference/lookup entities (used for seeding in tests)
    IRepository<ChronicDisease>   ChronicDiseases   { get; }
    IRepository<Specialization>   Specializations   { get; }
    IRepository<SubscriptionPlan> SubscriptionPlans { get; }
    IUserSeedRepository           UserEntities      { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
