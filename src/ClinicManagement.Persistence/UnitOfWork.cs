using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;

namespace ClinicManagement.Persistence;

/// <summary>
/// Aggregates all repositories and owns SaveChangesAsync.
/// Repositories are injected by DI — UnitOfWork never news them up directly.
/// This means swapping any repository (e.g. EF → Dapper) is a one-line change
/// in DependencyInjection.cs with zero changes here.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IPatientRepository            Patients            { get; }
    public IClinicMemberRepository       Members             { get; }
    public IDoctorInfoRepository         DoctorInfos         { get; }
    public IDoctorScheduleRepository     DoctorSchedules     { get; }
    public IInvitationRepository         Invitations         { get; }
    public IClinicRepository             Clinics             { get; }
    public IBranchRepository             Branches            { get; }
    public IUserRepository               Users               { get; }
    public IAuditLogRepository           AuditLogs           { get; }
    public IReferenceRepository          Reference           { get; }
    public IClinicSubscriptionRepository ClinicSubscriptions { get; }
    public IGeoLocationRepository        GeoLocations        { get; }
    public IPermissionRepository         Permissions         { get; }
    public IPatientCounterRepository     PatientCounters     { get; }
    public IChronicDiseaseRepository     ChronicDiseases     { get; }
    public ISpecializationRepository     Specializations     { get; }
    public ISubscriptionPlanRepository   SubscriptionPlans   { get; }
    public IRefreshTokenRepository       RefreshTokens       { get; }
    public ITestimonialRepository        Testimonials        { get; }
    public IContactMessageRepository     ContactMessages     { get; }
    public IAppointmentRepository        Appointments        { get; }
    public IQueueCounterRepository       QueueCounters       { get; }
    public IDoctorSessionRepository      DoctorSessions      { get; }
    public INotificationRepository       Notifications       { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        IPatientRepository            patients,
        IClinicMemberRepository       members,
        IDoctorInfoRepository         doctorInfos,
        IDoctorScheduleRepository     doctorSchedules,
        IInvitationRepository         invitations,
        IClinicRepository             clinics,
        IBranchRepository             branches,
        IUserRepository               users,
        IAuditLogRepository           auditLogs,
        IReferenceRepository          reference,
        IClinicSubscriptionRepository clinicSubscriptions,
        IGeoLocationRepository        geoLocations,
        IPermissionRepository         permissions,
        IPatientCounterRepository     patientCounters,
        IChronicDiseaseRepository     chronicDiseases,
        ISpecializationRepository     specializations,
        ISubscriptionPlanRepository   subscriptionPlans,
        IRefreshTokenRepository       refreshTokens,
        ITestimonialRepository        testimonials,
        IContactMessageRepository     contactMessages,
        IAppointmentRepository        appointments,
        IQueueCounterRepository       queueCounters,
        IDoctorSessionRepository      doctorSessions,
        INotificationRepository       notifications)
    {
        _context            = context;
        Patients            = patients;
        Members             = members;
        DoctorInfos         = doctorInfos;
        DoctorSchedules     = doctorSchedules;
        Invitations         = invitations;
        Clinics             = clinics;
        Branches            = branches;
        Users               = users;
        AuditLogs           = auditLogs;
        Reference           = reference;
        ClinicSubscriptions = clinicSubscriptions;
        GeoLocations        = geoLocations;
        Permissions         = permissions;
        PatientCounters     = patientCounters;
        ChronicDiseases     = chronicDiseases;
        Specializations     = specializations;
        SubscriptionPlans   = subscriptionPlans;
        RefreshTokens       = refreshTokens;
        Testimonials        = testimonials;
        ContactMessages     = contactMessages;
        Appointments        = appointments;
        QueueCounters       = queueCounters;
        DoctorSessions      = doctorSessions;
        Notifications       = notifications;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
