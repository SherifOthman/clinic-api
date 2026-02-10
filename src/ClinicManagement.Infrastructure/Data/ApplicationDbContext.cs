using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Entities.Outbox;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPublisher _publisher;

    public DbSet<ChronicDisease> ChronicDiseases => Set<ChronicDisease>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorWorkingDay> DoctorWorkingDays => Set<DoctorWorkingDay>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientPhone> PatientPhones => Set<PatientPhone>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicBranch> ClinicBranches => Set<ClinicBranch>();
    public DbSet<PatientChronicDisease> PatientChronicDiseases => Set<PatientChronicDisease>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalFile> MedicalFiles => Set<MedicalFile>();
    public DbSet<ClinicBranchAppointmentPrice> ClinicBranchAppointmentPrices => Set<ClinicBranchAppointmentPrice>();
    public DbSet<AppointmentType> AppointmentTypes => Set<AppointmentType>();
    
    // User type entities
    public DbSet<ClinicOwner> ClinicOwners => Set<ClinicOwner>();
    public DbSet<Receptionist> Receptionists => Set<Receptionist>();
    
    public DbSet<ClinicBranchPhoneNumber> ClinicBranchPhoneNumbers => Set<ClinicBranchPhoneNumber>();
    
    // Staff invitation
    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();
    
    // New pharmacy and billing entities
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicalSupply> MedicalSupplies => Set<MedicalSupply>();
    public DbSet<MedicalService> MedicalServices => Set<MedicalService>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    
    // Outbox pattern for reliable event publishing
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    
    // Measurement entities
    public DbSet<MeasurementAttribute> MeasurementAttributes => Set<MeasurementAttribute>();
    public DbSet<MedicalVisitMeasurement> MedicalVisitMeasurements => Set<MedicalVisitMeasurement>();
    public DbSet<DoctorMeasurementAttribute> DoctorMeasurementAttributes => Set<DoctorMeasurementAttribute>();
    public DbSet<SpecializationMeasurementAttribute> SpecializationMeasurementAttributes => Set<SpecializationMeasurementAttribute>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider, IPublisher publisher) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
        _publisher = publisher;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update audit fields
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = _dateTimeProvider.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = _dateTimeProvider.UtcNow;
            }
        }

        // Collect domain events before saving
        var aggregatesWithEvents = ChangeTracker.Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Save changes to database
        var result = await base.SaveChangesAsync(cancellationToken);

        // Dispatch domain events AFTER successful save
        // This ensures events are only published if the transaction succeeds
        await DispatchDomainEventsAsync(aggregatesWithEvents, cancellationToken);

        return result;
    }

    /// <summary>
    /// Dispatches domain events for all aggregates that have events
    /// Events are dispatched AFTER the transaction commits to ensure consistency
    /// </summary>
    private async Task DispatchDomainEventsAsync(List<AggregateRoot> aggregates, CancellationToken cancellationToken)
    {
        foreach (var aggregate in aggregates)
        {
            var events = aggregate.DomainEvents.ToList();
            aggregate.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>().ToTable("Users");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // EF 10 Named Query Filters
        ConfigureNamedQueryFilters(builder);
    }

    private void ConfigureNamedQueryFilters(ModelBuilder builder)
    {
        // EF 10 Named Query Filters for Multi-tenancy and Soft Delete
        // Pattern: TenantFilter isolates data by clinic, SoftDeleteFilter hides deleted records
        
        // ============================================================================
        // PATIENT AND RELATED ENTITIES
        // ============================================================================
        
        builder.Entity<Patient>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, p => _currentUserService.ClinicId == null || p.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, p => !p.IsDeleted);

        builder.Entity<PatientChronicDisease>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, pcd => _currentUserService.ClinicId == null || pcd.Patient.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, pcd => !pcd.IsDeleted);

        builder.Entity<PatientPhone>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, pp => _currentUserService.ClinicId == null || pp.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalFile>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mf => _currentUserService.ClinicId == null || mf.Patient.ClinicId == _currentUserService.ClinicId);

        // ============================================================================
        // CLINIC AND RELATED ENTITIES
        // ============================================================================
        
        builder.Entity<Clinic>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, c => _currentUserService.ClinicId == null || c.Id == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, c => !c.IsDeleted);
        
        builder.Entity<ClinicBranch>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, cb => _currentUserService.ClinicId == null || cb.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, cb => !cb.IsDeleted);

        builder.Entity<ClinicBranchPhoneNumber>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, cbpn => _currentUserService.ClinicId == null || cbpn.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        builder.Entity<ClinicBranchAppointmentPrice>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, cbap => _currentUserService.ClinicId == null || cbap.ClinicBranch.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, cbap => !cbap.IsDeleted);

        // ============================================================================
        // USER TYPE ENTITIES (Doctor, Receptionist, ClinicOwner)
        // ============================================================================
        
        // Note: User entity filter is commented out to allow cross-clinic user queries if needed
        //builder.Entity<User>()
        //    .HasQueryFilter(QueryFilterConstants.TenantFilter, u => _currentUserService.ClinicId == null || u.ClinicId == _currentUserService.ClinicId);

        builder.Entity<ClinicOwner>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, co => _currentUserService.ClinicId == null || co.User.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, co => !co.IsDeleted);

        builder.Entity<Doctor>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, d => _currentUserService.ClinicId == null || d.User.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, d => !d.IsDeleted);

        builder.Entity<Receptionist>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, r => _currentUserService.ClinicId == null || r.User.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, r => !r.IsDeleted);

        builder.Entity<DoctorWorkingDay>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, dwd => _currentUserService.ClinicId == null || dwd.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        // ============================================================================
        // AUTHENTICATION AND AUTHORIZATION
        // ============================================================================
        
        builder.Entity<RefreshToken>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, rt => _currentUserService.ClinicId == null || rt.User.ClinicId == _currentUserService.ClinicId);

        builder.Entity<StaffInvitation>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, si => _currentUserService.ClinicId == null || si.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, si => !si.IsDeleted);

        // ============================================================================
        // APPOINTMENT AND MEDICAL VISIT ENTITIES
        // ============================================================================
        
        builder.Entity<Appointment>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, a => _currentUserService.ClinicId == null || a.Patient.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, a => !a.IsDeleted);

        builder.Entity<MedicalVisit>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mv => _currentUserService.ClinicId == null || mv.Appointment.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalVisitMeasurement>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mvm => _currentUserService.ClinicId == null || mvm.MedicalVisit.Appointment.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalVisitRadiology>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mvr => _currentUserService.ClinicId == null || mvr.MedicalVisit.Appointment.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<Prescription>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, p => _currentUserService.ClinicId == null || p.Visit.Appointment.Patient.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, p => !p.IsDeleted);

        builder.Entity<PrescriptionItem>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, pi => _currentUserService.ClinicId == null || pi.Prescription.Visit.Appointment.Patient.ClinicId == _currentUserService.ClinicId);

        // ============================================================================
        // LAB AND RADIOLOGY TESTS
        // ============================================================================
        
        builder.Entity<LabTest>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, lt => _currentUserService.ClinicId == null || lt.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalVisitLabTest>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mvlt => _currentUserService.ClinicId == null || mvlt.LabTest.ClinicId == _currentUserService.ClinicId);

        builder.Entity<RadiologyTest>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, rt => _currentUserService.ClinicId == null || rt.ClinicId == _currentUserService.ClinicId);

        // ============================================================================
        // MEASUREMENT ATTRIBUTES
        // ============================================================================
        
        builder.Entity<DoctorMeasurementAttribute>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, dma => _currentUserService.ClinicId == null || dma.Doctor.User.ClinicId == _currentUserService.ClinicId);

        // ============================================================================
        // BILLING ENTITIES (Invoice, Payment)
        // ============================================================================
        
        builder.Entity<Invoice>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, i => _currentUserService.ClinicId == null || i.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, i => !i.IsDeleted);

        builder.Entity<InvoiceItem>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, ii => _currentUserService.ClinicId == null || ii.Invoice.ClinicId == _currentUserService.ClinicId);

        builder.Entity<Payment>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, p => _currentUserService.ClinicId == null || p.Invoice.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, p => !p.IsDeleted);

        // ============================================================================
        // INVENTORY ENTITIES (Medicine, MedicalService, MedicalSupply)
        // ============================================================================
        
        builder.Entity<Medicine>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, m => _currentUserService.ClinicId == null || m.ClinicBranch.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, m => !m.IsDeleted);

        builder.Entity<MedicalService>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, ms => _currentUserService.ClinicId == null || ms.ClinicBranch.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, ms => !ms.IsDeleted);

        builder.Entity<MedicalSupply>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, ms => _currentUserService.ClinicId == null || ms.ClinicBranch.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, ms => !ms.IsDeleted);
    }
}