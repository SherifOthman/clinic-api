using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

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
    
    // Measurement entities
    public DbSet<MeasurementAttribute> MeasurementAttributes => Set<MeasurementAttribute>();
    public DbSet<MedicalVisitMeasurement> MedicalVisitMeasurements => Set<MedicalVisitMeasurement>();
    public DbSet<DoctorMeasurementAttribute> DoctorMeasurementAttributes => Set<DoctorMeasurementAttribute>();
    public DbSet<SpecializationMeasurementAttribute> SpecializationMeasurementAttributes => Set<SpecializationMeasurementAttribute>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService, IDateTimeProvider dateTimeProvider) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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

        return await base.SaveChangesAsync(cancellationToken);   
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
        // EF 10 Named Query Filters for Multi-tenancy
        
        // Patient and related entities
        builder.Entity<Patient>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, p => _currentUserService.ClinicId == null || p.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, p => !p.IsDeleted);

        builder.Entity<PatientChronicDisease>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, pcd => _currentUserService.ClinicId == null || pcd.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<PatientPhone>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, pp => _currentUserService.ClinicId == null || pp.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalFile>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mf => _currentUserService.ClinicId == null || mf.Patient.ClinicId == _currentUserService.ClinicId);

        // Clinic and related entities
        builder.Entity<Clinic>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, c => _currentUserService.ClinicId == null || c.Id == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, c => !c.IsDeleted);

        //builder.Entity<User>()
        //    .HasQueryFilter(QueryFilterConstants.TenantFilter, u => _currentUserService.ClinicId == null || u.ClinicId == _currentUserService.ClinicId);

        builder.Entity<ClinicOwner>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, co => _currentUserService.ClinicId == null || co.User.ClinicId == _currentUserService.ClinicId);

        builder.Entity<Doctor>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, d => _currentUserService.ClinicId == null || d.User.ClinicId == _currentUserService.ClinicId);

        builder.Entity<Receptionist>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, r => _currentUserService.ClinicId == null || r.User.ClinicId == _currentUserService.ClinicId);

        builder.Entity<RefreshToken>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, rt => _currentUserService.ClinicId == null || rt.User.ClinicId == _currentUserService.ClinicId);

        builder.Entity<StaffInvitation>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, si => _currentUserService.ClinicId == null || si.ClinicId == _currentUserService.ClinicId);

        builder.Entity<LabTest>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, lt => _currentUserService.ClinicId == null || lt.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalVisitLabTest>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mvlt => _currentUserService.ClinicId == null || mvlt.LabTest.ClinicId == _currentUserService.ClinicId);

        builder.Entity<RadiologyTest>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, rt => _currentUserService.ClinicId == null || rt.ClinicId == _currentUserService.ClinicId);

        // ClinicBranch and related entities
        builder.Entity<ClinicBranch>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, cb => _currentUserService.ClinicId == null || cb.ClinicId == _currentUserService.ClinicId);

        builder.Entity<ClinicBranchPhoneNumber>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, cbpn => _currentUserService.ClinicId == null || cbpn.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        builder.Entity<ClinicBranchAppointmentPrice>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, cbap => _currentUserService.ClinicId == null || cbap.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        builder.Entity<DoctorWorkingDay>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, dwd => _currentUserService.ClinicId == null || dwd.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        // Appointment and related entities
        builder.Entity<Appointment>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, a => _currentUserService.ClinicId == null || a.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalVisit>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mv => _currentUserService.ClinicId == null || mv.Appointment.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalVisitMeasurement>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mvm => _currentUserService.ClinicId == null || mvm.MedicalVisit.Appointment.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalVisitRadiology>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, mvr => _currentUserService.ClinicId == null || mvr.MedicalVisit.Appointment.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<Prescription>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, p => _currentUserService.ClinicId == null || p.Visit.Appointment.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<PrescriptionItem>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, pi => _currentUserService.ClinicId == null || pi.Prescription.Visit.Appointment.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<DoctorMeasurementAttribute>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, dma => _currentUserService.ClinicId == null || dma.Doctor.User.ClinicId == _currentUserService.ClinicId);

        // Invoice and related entities
        builder.Entity<Invoice>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, i => _currentUserService.ClinicId == null || i.ClinicId == _currentUserService.ClinicId);

        builder.Entity<InvoiceItem>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, ii => _currentUserService.ClinicId == null || ii.Invoice.ClinicId == _currentUserService.ClinicId);

        builder.Entity<Payment>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, p => _currentUserService.ClinicId == null || p.Invoice.ClinicId == _currentUserService.ClinicId);

        // Inventory entities (Medicine, MedicalService, MedicalSupply)
        builder.Entity<Medicine>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, m => _currentUserService.ClinicId == null || m.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalService>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, ms => _currentUserService.ClinicId == null || ms.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalSupply>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, ms => _currentUserService.ClinicId == null || ms.ClinicBranch.ClinicId == _currentUserService.ClinicId);
    }
}