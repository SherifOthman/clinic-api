using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService?.UserId;
        var ipAddress = _currentUserService?.IpAddress;
        var userRole = _currentUserService?.Roles.FirstOrDefault();
        var fullName = _currentUserService?.FullName;
        var username = _currentUserService?.Username;
        var userEmail = _currentUserService?.UserEmail;
        var userAgent = _currentUserService?.UserAgent;
        var now = DateTime.UtcNow;

        var auditEntries = BuildAuditEntries(userId, fullName, username, userEmail, userAgent, ipAddress, userRole, now);

        // Stamp audit fields
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        // Persist audit logs after save (so EntityId is available for Added entries)
        if (auditEntries.Count > 0)
        {
            AuditLogs.AddRange(auditEntries);
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private List<AuditLog> BuildAuditEntries(Guid? userId, string? fullName, string? username, string? userEmail, string? userAgent, string? ipAddress, string? userRole, DateTime now)
    {
        var entries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            if (entry.Entity is AuditLog) continue;
            if (entry.Entity is RefreshToken) continue;

            // Detect soft-delete / soft-restore via IsDeleted flag change
            AuditAction action;
            if (entry.State == EntityState.Added)
            {
                action = AuditAction.Create;
            }
            else if (entry.State == EntityState.Deleted)
            {
                action = AuditAction.Delete;
            }
            else
            {
                // Modified — check if IsDeleted changed
                var isDeletedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");
                if (isDeletedProp is { IsModified: true })
                {
                    var wasDeleted = isDeletedProp.OriginalValue is true;
                    var isNowDeleted = isDeletedProp.CurrentValue is true;
                    action = (wasDeleted, isNowDeleted) switch
                    {
                        (false, true)  => AuditAction.Delete,   // soft-delete
                        (true,  false) => AuditAction.Restore,  // soft-restore
                        _              => AuditAction.Update,
                    };
                }
                else
                {
                    action = AuditAction.Update;
                }
            }

            string? changesJson = null;
            if (action == AuditAction.Update)
            {
                var changes = new Dictionary<string, object>();
                foreach (var prop in entry.Properties)
                {
                    if (!prop.IsModified) continue;
                    if (prop.Metadata.Name is "UpdatedAt" or "UpdatedBy" or "IsDeleted") continue;
                    changes[prop.Metadata.Name] = new
                    {
                        Old = HumanizeValue(prop.Metadata.Name, prop.OriginalValue),
                        New = HumanizeValue(prop.Metadata.Name, prop.CurrentValue),
                    };
                }
                if (changes.Count > 0)
                    changesJson = System.Text.Json.JsonSerializer.Serialize(changes);
            }
            else if (action == AuditAction.Create)
            {
                var snapshot = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.Name is "CreatedAt" or "CreatedBy" or "UpdatedAt" or "UpdatedBy" or "IsDeleted") continue;
                    if (prop.CurrentValue is null) continue;
                    var humanized = HumanizeValue(prop.Metadata.Name, prop.CurrentValue);
                    if (humanized is null) continue;
                    snapshot[prop.Metadata.Name] = humanized;
                }
                if (snapshot.Count > 0)
                    changesJson = System.Text.Json.JsonSerializer.Serialize(snapshot);
            }
            else if (action == AuditAction.Delete || action == AuditAction.Restore)
            {
                var snapshot = new Dictionary<string, object?>();
                foreach (var prop in entry.Properties)
                {
                    if (prop.Metadata.Name is "CreatedAt" or "CreatedBy" or "UpdatedAt" or "UpdatedBy" or "IsDeleted") continue;
                    var val = entry.State == EntityState.Deleted ? prop.OriginalValue : prop.CurrentValue;
                    if (val is null) continue;
                    var humanized = HumanizeValue(prop.Metadata.Name, val);
                    if (humanized is null) continue;
                    snapshot[prop.Metadata.Name] = humanized;
                }
                if (snapshot.Count > 0)
                    changesJson = System.Text.Json.JsonSerializer.Serialize(snapshot);
            }

            Guid? clinicId = null;
            if (entry.Entity is ITenantEntity tenantEntity)
                clinicId = tenantEntity.ClinicId;

            entries.Add(new AuditLog
            {
                Timestamp  = now,
                ClinicId   = clinicId,
                UserId     = userId,
                FullName   = fullName,
                Username   = username,
                UserEmail  = userEmail,
                UserRole   = userRole,
                UserAgent  = userAgent,
                EntityType = entry.Entity.GetType().Name,
                EntityId   = entry.Entity.Id.ToString(),
                Action     = action,
                IpAddress  = ipAddress,
                Changes    = changesJson,
            });
        }

        return entries;
    }

    /// <summary>Converts raw EF values to human-readable strings for audit logs.</summary>
    private static object? HumanizeValue(string fieldName, object? value)
    {
        if (value is null) return null;

        // Boolean fields — map to meaningful labels
        return fieldName switch
        {
            "IsMale"    => value is bool b ? (b ? "Male" : "Female") : value,
            "IsActive"  => value is bool a ? (a ? "Active" : "Inactive") : value,
            "IsDeleted" => value is bool d ? (d ? "Deleted" : "Active") : value,
            "IsRevoked" => value is bool r ? (r ? "Revoked" : "Active") : value,
            "BloodType" => value is int bt ? bt switch
            {
                1 => "A+", 2 => "A-", 3 => "B+", 4 => "B-",
                5 => "AB+", 6 => "AB-", 7 => "O+", 8 => "O-",
                _ => value
            } : value,
            // Skip internal IDs — not useful for humans
            "ClinicId" or "UserId" or "PatientId" or "StaffId" or "CreatedBy" or "UpdatedBy" => null,
            _ => value
        };
    }

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

        // Apply tenant filter to all ITenantEntity types automatically
        // Apply soft-delete filter to all AuditableEntity types automatically
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(clrType)
                    .Invoke(this, [modelBuilder]);
            }

            if (typeof(AuditableEntity).IsAssignableFrom(clrType))
            {
                typeof(ApplicationDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(clrType)
                    .Invoke(null, [modelBuilder]);
            }
        }
    }

    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(
                QueryFilterNames.Tenant,
                e => e.ClinicId == (_currentUserService!.ClinicId ?? Guid.Empty));
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : AuditableEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(QueryFilterNames.SoftDelete, e => !e.IsDeleted);
    }
}
