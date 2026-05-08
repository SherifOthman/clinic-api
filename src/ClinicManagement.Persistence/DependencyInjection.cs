using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence.Audit;
using ClinicManagement.Persistence.Jobs;
using ClinicManagement.Persistence.Repositories;
using ClinicManagement.Persistence.Seeders;
using ClinicManagement.Persistence.Seeders.Demo;using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseSqlServer(connectionString)
                .ConfigureWarnings(w => w.Ignore(
                    CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

        // Unit of Work (aggregates all repositories — receives them via DI constructor injection)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // AuditChangeTracker is an instance class so it can be injected into ApplicationDbContext,
        // mocked in tests, and extended without modifying the DbContext.
        services.AddScoped<AuditChangeTracker>();

        // All repositories registered individually so:
        // 1. UnitOfWork receives them via constructor injection (no `new` inside UoW)
        // 2. Any repo can be swapped to a different implementation (Dapper, ADO.NET, etc.)
        //    by changing only this file — nothing else needs to change.
        // 3. IPermissionRepository is also resolved directly by PermissionAuthorizationHandler
        //    (which runs outside MediatR, before IUnitOfWork is created).
        services.AddScoped<IPatientRepository,            PatientRepository>();
        services.AddScoped<IClinicMemberRepository,       ClinicMemberRepository>();
        services.AddScoped<IDoctorInfoRepository,         DoctorInfoRepository>();
        services.AddScoped<IDoctorScheduleRepository,     DoctorScheduleRepository>();
        services.AddScoped<IInvitationRepository,         InvitationRepository>();
        services.AddScoped<IClinicRepository,             ClinicRepository>();
        services.AddScoped<IBranchRepository,             BranchRepository>();
        services.AddScoped<IUserRepository,               UserRepository>();
        services.AddScoped<IAuditLogRepository,           AuditLogRepository>();
        services.AddScoped<IReferenceRepository,          ReferenceRepository>();
        services.AddScoped<IClinicSubscriptionRepository, ClinicSubscriptionRepository>();
        services.AddScoped<IGeoLocationRepository,        GeoLocationRepository>();
        services.AddScoped<IPermissionRepository,         PermissionRepository>();
        services.AddScoped<IPatientCounterRepository,     PatientCounterRepository>();
        services.AddScoped<IChronicDiseaseRepository,     ChronicDiseaseRepository>();
        services.AddScoped<ISpecializationRepository,     SpecializationRepository>();
        services.AddScoped<ISubscriptionPlanRepository,   SubscriptionPlanRepository>();
        services.AddScoped<IRefreshTokenRepository,       RefreshTokenRepository>();
        services.AddScoped<ITestimonialRepository,        TestimonialRepository>();
        services.AddScoped<IContactMessageRepository,     ContactMessageRepository>();
        services.AddScoped<IAppointmentRepository,        AppointmentRepository>();
        services.AddScoped<IQueueCounterRepository,       QueueCounterRepository>();
        services.AddScoped<IDoctorSessionRepository,      DoctorSessionRepository>();
        services.AddScoped<INotificationRepository,       NotificationRepository>();

        services.AddIdentity<User, Role>(options =>
        {
            options.Password.RequireDigit           = true;
            options.Password.RequireLowercase       = true;
            options.Password.RequireUppercase       = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength         = 8;

            options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(30);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers      = true;

            options.User.RequireUniqueEmail         = true;
            options.SignIn.RequireConfirmedEmail     = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<RoleSeedService>();
        services.AddScoped<SpecializationSeedService>();
        services.AddScoped<ChronicDiseaseSeedService>();
        services.AddScoped<SubscriptionPlanSeedService>();
        services.AddScoped<SystemUserSeedService>();
        services.AddScoped<GeoLocationSeedService>();

        // Demo data seeders — only registered, DatabaseInitialiser decides whether to run them
        services.AddScoped<DemoClinicSeeder>();
        services.AddScoped<DemoPatientsSeeder>();
        services.AddScoped<DemoAppointmentsSeeder>();
        services.AddScoped<DemoContactSeeder>();
        services.AddScoped<DemoTestimonialsSeeder>();
        services.AddScoped<DemoNotificationsSeeder>();
        services.AddScoped<DemoAuditSeeder>();
        services.AddScoped<DemoInvitationsSeeder>();
        services.AddScoped<DemoUsageMetricsSeeder>();
        services.AddScoped<DemoDataSeedService>();

        // Hangfire jobs — data-access-heavy jobs belong in Persistence
        services.AddScoped<EmailQueueProcessorJob>();
        services.AddScoped<AuditLogCleanupService>();
        services.AddScoped<UsageMetricsAggregationJob>();
        services.AddScoped<SubscriptionExpiryNotificationJob>();
        services.AddScoped<UsageLimitNotificationJob>();

        return services;
    }
}
