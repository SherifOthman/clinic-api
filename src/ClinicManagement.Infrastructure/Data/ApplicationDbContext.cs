using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
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
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicBranch> ClinicBranches => Set<ClinicBranch>();
    public DbSet<ClinicPatient> ClinicPatients => Set<ClinicPatient>();
    public DbSet<ClinicPatientChronicDisease> ClinicPatientChronicDiseases => Set<ClinicPatientChronicDisease>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalFile> MedicalFiles => Set<MedicalFile>();
    public DbSet<ClinicBranchAppointmentPrice> ClinicBranchAppointmentPrices => Set<ClinicBranchAppointmentPrice>();
    public DbSet<AppointmentType> AppointmentTypes => Set<AppointmentType>();
    
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
    }
}
