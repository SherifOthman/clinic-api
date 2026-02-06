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
    
    // Location entities
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
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
        builder.Entity<Patient>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, p => _currentUserService.ClinicId == null || p.ClinicId == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, p => !p.IsDeleted);

        builder.Entity<ClinicBranch>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, cb => _currentUserService.ClinicId == null || cb.ClinicId == _currentUserService.ClinicId);

        builder.Entity<Clinic>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, c => _currentUserService.ClinicId == null || c.Id == _currentUserService.ClinicId)
            .HasQueryFilter(QueryFilterConstants.SoftDeleteFilter, c => !c.IsDeleted);

        builder.Entity<Appointment>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, a => _currentUserService.ClinicId == null || a.Patient.ClinicId == _currentUserService.ClinicId);

        builder.Entity<Invoice>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, i => _currentUserService.ClinicId == null || i.ClinicId == _currentUserService.ClinicId);

        builder.Entity<Medicine>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, m => _currentUserService.ClinicId == null || m.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalService>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, ms => _currentUserService.ClinicId == null || ms.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        builder.Entity<MedicalSupply>()
            .HasQueryFilter(QueryFilterConstants.TenantFilter, ms => _currentUserService.ClinicId == null || ms.ClinicBranch.ClinicId == _currentUserService.ClinicId);
    }
}