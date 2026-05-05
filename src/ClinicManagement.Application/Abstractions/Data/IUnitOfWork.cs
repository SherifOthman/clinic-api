using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Data;

/// <summary>
/// Single entry point for all data access in handlers.
/// Handlers inject ONLY IUnitOfWork — no direct DbContext access.
/// </summary>
public interface IUnitOfWork
{
    IPatientRepository            Patients            { get; }
    IClinicMemberRepository       Members             { get; }
    IDoctorInfoRepository         DoctorInfos         { get; }
    IDoctorScheduleRepository     DoctorSchedules     { get; }
    IInvitationRepository         Invitations         { get; }
    IClinicRepository             Clinics             { get; }
    IBranchRepository             Branches            { get; }
    IUserRepository               Users               { get; }
    IAuditLogRepository           AuditLogs           { get; }
    IReferenceRepository          Reference           { get; }
    IClinicSubscriptionRepository ClinicSubscriptions { get; }
    IGeoLocationRepository        GeoLocations        { get; }
    IPermissionRepository         Permissions         { get; }
    IPatientCounterRepository     PatientCounters     { get; }

    IChronicDiseaseRepository     ChronicDiseases   { get; }
    ISpecializationRepository     Specializations   { get; }
    ISubscriptionPlanRepository   SubscriptionPlans { get; }
    IRefreshTokenRepository       RefreshTokens     { get; }
    ITestimonialRepository        Testimonials      { get; }
    IContactMessageRepository     ContactMessages   { get; }
    IAppointmentRepository        Appointments      { get; }
    IQueueCounterRepository       QueueCounters     { get; }
    IDoctorSessionRepository      DoctorSessions    { get; }
    INotificationRepository       Notifications     { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
