using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ClinicManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly CurrentUserService _currentUserService;
    private readonly DateTimeProvider _dateTimeProvider;

    public DbSet<ChronicDisease> ChronicDiseases => Set<ChronicDisease>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    
    // Staff architecture
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    
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
    
    public DbSet<ClinicBranchPhoneNumber> ClinicBranchPhoneNumbers => Set<ClinicBranchPhoneNumber>();
    
    // Staff invitation
    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();
    
    // Pharmacy and billing entities
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicalSupply> MedicalSupplies => Set<MedicalSupply>();
    public DbSet<MedicalService> MedicalServices => Set<MedicalService>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    
    // Measurement entities
    public DbSet<MeasurementAttribute> MeasurementAttributes => Set<MeasurementAttribute>();
    public DbSet<MedicalVisitMeasurement> MedicalVisitMeasurements => Set<MedicalVisitMeasurement>();
    public DbSet<DoctorMeasurementAttribute> DoctorMeasurementAttributes => Set<DoctorMeasurementAttribute>();
    public DbSet<SpecializationMeasurementAttribute> SpecializationMeasurementAttributes => Set<SpecializationMeasurementAttribute>();

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        CurrentUserService currentUserService,
        DateTimeProvider dateTimeProvider) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        var currentClinicId = _currentUserService.ClinicId;
        var now = _dateTimeProvider.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                    break;
                    
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    break;
                    
                case EntityState.Deleted:
                    // Soft delete instead of hard delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = currentUserId;
                    break;
            }
        }
        
        // Automatically set ClinicId for new tenant-scoped entities
        // This ensures multi-tenancy isolation at the data layer
        if (currentClinicId.HasValue)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    // Only set if not already set (allows explicit override)
                    if (entry.Entity.ClinicId == Guid.Empty)
                    {
                        entry.Entity.ClinicId = currentClinicId.Value;
                    }
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure Identity tables with custom schema
        builder.Entity<User>().ToTable("Users", "Identity");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles", "Identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "Identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "Identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "Identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "Identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "Identity");
        
        // ===== GLOBAL QUERY FILTERS =====
        ConfigureGlobalQueryFilters(builder);
    }
    
    /// <summary>
    /// Configures global query filters for multi-tenancy and soft delete
    /// Multi-tenancy: Filters by ClinicId from JWT claims (null for SuperAdmin = see all)
    /// Soft delete: Excludes IsDeleted=true records (use IncludeDeleted() to bypass)
    /// Child entities inherit filters from their parents to prevent orphaned records
    /// </summary>
    private void ConfigureGlobalQueryFilters(ModelBuilder builder)
    {
        // Get current user's clinic ID (null for SuperAdmin who can see all clinics)
        var clinicId = _currentUserService.ClinicId;
        
        // ===== NEW STAFF ARCHITECTURE =====
        
        // Staff - clinic membership
        builder.Entity<Staff>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.ClinicId == clinicId));
        
        // DoctorProfile - filtered through Staff
        builder.Entity<DoctorProfile>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.Staff.ClinicId == clinicId));
        
        // ===== PARENT ENTITIES WITH SOFT DELETE =====
        
        // Clinic-scoped entities with soft delete
        builder.Entity<Patient>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.ClinicId == clinicId));
        
        builder.Entity<Invoice>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.ClinicId == clinicId));
        
        builder.Entity<StaffInvitation>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.ClinicId == clinicId));
        
        // Branch-scoped entities with soft delete
        builder.Entity<Appointment>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.ClinicBranch.ClinicId == clinicId));
        
        builder.Entity<Medicine>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.ClinicBranch.ClinicId == clinicId));
        
        builder.Entity<MedicalService>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.ClinicBranch.ClinicId == clinicId));
        
        builder.Entity<MedicalSupply>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.ClinicBranch.ClinicId == clinicId));
        
        builder.Entity<ClinicBranch>().HasQueryFilter(e => 
            !e.IsDeleted && (clinicId == null || e.ClinicId == clinicId));
        
        // Clinic entity itself (only soft delete filter)
        builder.Entity<Clinic>().HasQueryFilter(e => !e.IsDeleted);
        
        // Medical entities with soft delete
        builder.Entity<PatientChronicDisease>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ClinicBranchAppointmentPrice>().HasQueryFilter(e => !e.IsDeleted);
        
        // ===== CHILD ENTITIES - INHERIT PARENT FILTERS =====
        // These entities are filtered through their parent relationships to prevent orphaned records
        
        // Children of ClinicBranch
        builder.Entity<ClinicBranchPhoneNumber>().HasQueryFilter(e => 
            !e.ClinicBranch.IsDeleted);
        
        builder.Entity<DoctorWorkingDay>().HasQueryFilter(e => 
            !e.ClinicBranch.IsDeleted);
        
        builder.Entity<LabTestOrder>().HasQueryFilter(e => 
            !e.ClinicBranch.IsDeleted);
        
        builder.Entity<RadiologyOrder>().HasQueryFilter(e => 
            !e.ClinicBranch.IsDeleted);
        
        builder.Entity<MedicineDispensing>().HasQueryFilter(e => 
            !e.ClinicBranch.IsDeleted);
        
        // Children of Patient
        builder.Entity<PatientPhone>().HasQueryFilter(e => 
            !e.Patient.IsDeleted);
        
        builder.Entity<PatientAllergy>().HasQueryFilter(e => 
            !e.Patient.IsDeleted);
        
        builder.Entity<MedicalFile>().HasQueryFilter(e => 
            !e.Patient.IsDeleted);
        
        // Children of Invoice
        builder.Entity<InvoiceItem>().HasQueryFilter(e => 
            !e.Invoice.IsDeleted);
        
        // Children of Appointment
        builder.Entity<MedicalVisit>().HasQueryFilter(e => 
            !e.Appointment.IsDeleted);
        
        // Children of DoctorProfile
        builder.Entity<DoctorMeasurementAttribute>().HasQueryFilter(e => 
            !e.DoctorProfile.IsDeleted);
        
        // Children of MedicalVisit (join tables)
        builder.Entity<MedicalVisitLabTest>().HasQueryFilter(e => 
            !e.MedicalVisit.Appointment.IsDeleted);
        
        builder.Entity<MedicalVisitRadiology>().HasQueryFilter(e => 
            !e.MedicalVisit.Appointment.IsDeleted);
        
        builder.Entity<MedicalVisitMeasurement>().HasQueryFilter(e => 
            !e.MedicalVisit.Appointment.IsDeleted);
        
        // Children of Prescription
        builder.Entity<Prescription>().HasQueryFilter(e => 
            !e.Visit.Appointment.IsDeleted);
        
        builder.Entity<PrescriptionItem>().HasQueryFilter(e => 
            !e.Prescription.Visit.Appointment.IsDeleted);
        
        // ===== NO SOFT DELETE (BaseEntity) - Only tenant filter where applicable =====
        
        // Tenant-scoped BaseEntity entities
        builder.Entity<LabTest>().HasQueryFilter(e => 
            clinicId == null || e.ClinicId == clinicId);
        
        builder.Entity<RadiologyTest>().HasQueryFilter(e => 
            clinicId == null || e.ClinicId == clinicId);
        
        builder.Entity<ClinicMedication>().HasQueryFilter(e => 
            clinicId == null || e.ClinicId == clinicId);
    }
}
