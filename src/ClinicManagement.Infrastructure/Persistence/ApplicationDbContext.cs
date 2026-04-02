using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<IdentityUserRole<Guid>> UserRoles => Set<IdentityUserRole<Guid>>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<EmailQueue> EmailQueue => Set<EmailQueue>();

    // Reference Data
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<ChronicDisease> ChronicDiseases => Set<ChronicDisease>();

    // Clinic
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicBranch> ClinicBranches => Set<ClinicBranch>();
    public DbSet<ClinicBranchPhoneNumber> ClinicBranchPhoneNumbers => Set<ClinicBranchPhoneNumber>();
    public DbSet<ClinicBranchAppointmentPrice> ClinicBranchAppointmentPrices => Set<ClinicBranchAppointmentPrice>();
    public DbSet<ClinicSubscription> ClinicSubscriptions => Set<ClinicSubscription>();
    public DbSet<ClinicUsageMetrics> ClinicUsageMetrics => Set<ClinicUsageMetrics>();
    public DbSet<DoctorWorkingDay> DoctorWorkingDays => Set<DoctorWorkingDay>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();

    // Staff
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();

    // Patient
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientPhone> PatientPhones => Set<PatientPhone>();
    public DbSet<PatientAllergy> PatientAllergies => Set<PatientAllergy>();
    public DbSet<PatientChronicDisease> PatientChronicDiseases => Set<PatientChronicDisease>();

    // Appointment
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentType> AppointmentTypes => Set<AppointmentType>();

    // Medical
    public DbSet<MedicalVisit> MedicalVisits => Set<MedicalVisit>();
    public DbSet<MedicalFile> MedicalFiles => Set<MedicalFile>();
    public DbSet<PatientMedicalFile> PatientMedicalFiles => Set<PatientMedicalFile>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<LabTest> LabTests => Set<LabTest>();
    public DbSet<LabTestOrder> LabTestOrders => Set<LabTestOrder>();
    public DbSet<MedicalVisitLabTest> MedicalVisitLabTests => Set<MedicalVisitLabTest>();
    public DbSet<RadiologyTest> RadiologyTests => Set<RadiologyTest>();
    public DbSet<RadiologyOrder> RadiologyOrders => Set<RadiologyOrder>();
    public DbSet<MedicalVisitRadiology> MedicalVisitRadiologies => Set<MedicalVisitRadiology>();
    public DbSet<MeasurementAttribute> MeasurementAttributes => Set<MeasurementAttribute>();
    public DbSet<MedicalVisitMeasurement> MedicalVisitMeasurements => Set<MedicalVisitMeasurement>();
    public DbSet<DoctorMeasurementAttribute> DoctorMeasurementAttributes => Set<DoctorMeasurementAttribute>();
    public DbSet<SpecializationMeasurementAttribute> SpecializationMeasurementAttributes => Set<SpecializationMeasurementAttribute>();
    public DbSet<Medicine> Medicines => Set<Medicine>();
    public DbSet<MedicineDispensing> MedicineDispensings => Set<MedicineDispensing>();
    public DbSet<MedicalService> MedicalServices => Set<MedicalService>();
    public DbSet<MedicalSupply> MedicalSupplies => Set<MedicalSupply>();

    // Billing
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Rename Identity tables
        modelBuilder.Entity<User>().ToTable("Users");
        modelBuilder.Entity<Role>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Named clinic filter (EF Core 10).
        // Scopes all tenant entities to the current clinic automatically.
        // EF Core re-evaluates _currentUserService.ClinicId on every query.
        // Bypass with: query.IgnoreQueryFilters([QueryFilterNames.Clinic])
        modelBuilder.Entity<Staff>()
            .HasQueryFilter(QueryFilterNames.Clinic, s => s.ClinicId == (_currentUserService!.ClinicId ?? Guid.Empty));

        modelBuilder.Entity<StaffInvitation>()
            .HasQueryFilter(QueryFilterNames.Clinic, si => si.ClinicId == (_currentUserService!.ClinicId ?? Guid.Empty));

        modelBuilder.Entity<ClinicBranch>()
            .HasQueryFilter(QueryFilterNames.Clinic, b => b.ClinicId == (_currentUserService!.ClinicId ?? Guid.Empty));

        modelBuilder.Entity<Patient>()
            .HasQueryFilter(QueryFilterNames.Clinic, p => p.ClinicId == (_currentUserService!.ClinicId ?? Guid.Empty));

        modelBuilder.Entity<Invoice>()
            .HasQueryFilter(QueryFilterNames.Clinic, i => i.ClinicId == (_currentUserService!.ClinicId ?? Guid.Empty));

        modelBuilder.Entity<LabTest>()
            .HasQueryFilter(QueryFilterNames.Clinic, l => l.ClinicId == (_currentUserService!.ClinicId ?? Guid.Empty));

        modelBuilder.Entity<RadiologyTest>()
            .HasQueryFilter(QueryFilterNames.Clinic, r => r.ClinicId == (_currentUserService!.ClinicId ?? Guid.Empty));
    }
}
