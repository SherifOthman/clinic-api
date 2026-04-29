using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Data;

/// <summary>
/// Single entry point for all data access in handlers.
/// Handlers inject ONLY IUnitOfWork — no direct DbContext access.
/// </summary>
public interface IUnitOfWork
{
    IPatientRepository           Patients           { get; }
    IClinicMemberRepository      Members            { get; }
    IDoctorInfoRepository        DoctorInfos        { get; }
    IDoctorScheduleRepository    DoctorSchedules    { get; }
    IInvitationRepository        Invitations        { get; }
    IClinicRepository            Clinics            { get; }
    IBranchRepository            Branches           { get; }
    IUserRepository              Users              { get; }
    IAuditLogRepository          AuditLogs          { get; }
    IReferenceRepository         Reference          { get; }
    IClinicSubscriptionRepository ClinicSubscriptions { get; }
    IGeoLocationRepository       GeoLocations       { get; }
    IPermissionRepository        Permissions        { get; }
    IPatientCounterRepository    PatientCounters    { get; }

    // Generic repos for reference/lookup entities (used for seeding in tests)
    IRepository<ChronicDisease>   ChronicDiseases   { get; }
    IRepository<Specialization>   Specializations   { get; }
    IRepository<SubscriptionPlan> SubscriptionPlans { get; }
    IRepository<Person>           Persons           { get; }
    IRefreshTokenRepository       RefreshTokens     { get; }
    IUserSeedRepository           UserEntities      { get; }
    ITestimonialRepository        Testimonials      { get; }
    IContactMessageRepository     ContactMessages   { get; }
    IAppointmentRepository        Appointments      { get; }
    IQueueCounterRepository       QueueCounters     { get; }
    IDoctorSessionRepository      DoctorSessions    { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
