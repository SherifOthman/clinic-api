using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace ClinicManagement.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly IMemoryCache? _cache;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null,
        IMemoryCache? cache = null)
        : base(options)
    {
        _currentUserService = currentUserService;
        _cache = cache;
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
            // PatientPhone and PatientChronicDisease are AuditableEntity for timestamp tracking,
            // but they don't generate their own audit rows — they're included in the Patient snapshot.
            if (entry.Entity is PatientPhone) continue;
            if (entry.Entity is PatientChronicDisease) continue;

            // For Patient entities, enrich snapshot with phones and diseases from navigation properties
            Dictionary<string, object?>? patientExtras = null;
            if (entry.Entity is Patient patient)
            {
                patientExtras = new();
                if (patient.Phones?.Any() == true)
                    patientExtras["Phone Numbers"] = string.Join(", ", patient.Phones.Select(p => p.PhoneNumber));
                if (patient.ChronicDiseases?.Any() == true)
                {
                    // Resolve disease names from tracked ChronicDisease entities in context
                    var diseaseIds = patient.ChronicDiseases.Select(cd => cd.ChronicDiseaseId).ToList();
                    var diseaseNames = ChangeTracker.Entries<ChronicDisease>()
                        .Where(e => diseaseIds.Contains(e.Entity.Id))
                        .Select(e => e.Entity.NameEn)
                        .ToList();
                    // Fall back to IDs if not tracked
                    if (diseaseNames.Count > 0)
                        patientExtras["Chronic Diseases"] = string.Join(", ", diseaseNames);
                }
            }

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

            // Fields to always skip in audit output
            var skipFields = new HashSet<string>
            {
                "Id", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted",
                "ClinicId", "UserId", "PatientId", "StaffId", "DoctorProfileId",
                "AppointmentId", "InvoiceId", "MedicalVisitId"
            };

            string? changesJson = null;
            if (action == AuditAction.Update)
            {
                var changes = new Dictionary<string, object>();
                foreach (var prop in entry.Properties)
                {
                    if (!prop.IsModified) continue;
                    if (skipFields.Contains(prop.Metadata.Name)) continue;
                    var old = HumanizeValue(prop.Metadata.Name, prop.OriginalValue);
                    var @new = HumanizeValue(prop.Metadata.Name, prop.CurrentValue);
                    if (old is null && @new is null) continue;
                    var label = GetFieldLabel(prop.Metadata.Name);
                    changes[label] = new { Old = old, New = @new };
                }
                if (changes.Count > 0)
                    changesJson = System.Text.Json.JsonSerializer.Serialize(changes);
                // Append patient extras (phones, diseases) if available
                if (patientExtras != null && changesJson != null)
                {
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(changesJson) ?? new();
                    foreach (var kv in patientExtras) dict[kv.Key] = kv.Value;
                    changesJson = System.Text.Json.JsonSerializer.Serialize(dict);
                }
            }
            else if (action == AuditAction.Create)
            {
                var snapshot = BuildSnapshot(entry.Properties, skipFields, useOriginal: false);
                // Append patient extras (phones, diseases) if available
                if (patientExtras != null)
                    foreach (var kv in patientExtras) snapshot[kv.Key] = kv.Value;
                if (snapshot.Count > 0)
                    changesJson = System.Text.Json.JsonSerializer.Serialize(snapshot);
            }
            else if (action == AuditAction.Delete || action == AuditAction.Restore)
            {
                var snapshot = BuildSnapshot(entry.Properties, skipFields, useOriginal: entry.State == EntityState.Deleted);
                if (patientExtras != null)
                    foreach (var kv in patientExtras) snapshot[kv.Key] = kv.Value;
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

    private Dictionary<string, object?> BuildSnapshot(
        IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> properties,
        HashSet<string> skipFields,
        bool useOriginal)
    {
        // Collect raw values first, then group location fields
        var raw = new Dictionary<string, object?>();
        foreach (var prop in properties)
        {
            if (skipFields.Contains(prop.Metadata.Name)) continue;
            var val = useOriginal ? prop.OriginalValue : prop.CurrentValue;
            if (val is null) continue;
            var humanized = HumanizeValue(prop.Metadata.Name, val);
            if (humanized is null) continue;
            raw[prop.Metadata.Name] = humanized;
        }

        // Build ordered output — group location fields together
        var result = new Dictionary<string, object?>();
        var locationFields = new[] { "CountryGeoNameId", "StateGeoNameId", "CityGeoNameId" };

        // Add non-location fields first (in a logical order)
        var fieldOrder = new[] { "FullName", "IsMale", "DateOfBirth", "BloodType", "PatientCode",
                                  "Name", "Email", "PhoneNumber", "Role", "IsActive", "IsRevoked", "ExpiryTime", "EVENT" };

        foreach (var field in fieldOrder)
        {
            if (raw.TryGetValue(field, out var v))
                result[GetFieldLabel(field)] = v;
        }

        // Add location as a grouped entry
        var locationParts = new List<string>();
        if (raw.TryGetValue("CountryGeoNameId", out var country) && country is string cn) locationParts.Add(cn);
        if (raw.TryGetValue("StateGeoNameId", out var state) && state is string sn) locationParts.Add(sn);
        if (raw.TryGetValue("CityGeoNameId", out var city) && city is string ct) locationParts.Add(ct);
        if (locationParts.Count > 0)
            result["Location"] = string.Join(" › ", locationParts);

        // Add remaining fields not in the ordered list and not location
        foreach (var (key, val) in raw)
        {
            if (fieldOrder.Contains(key)) continue;
            if (locationFields.Contains(key)) continue;
            result[GetFieldLabel(key)] = val;
        }

        return result;
    }

    /// <summary>Maps C# property names to human-readable labels.</summary>
    private static string GetFieldLabel(string fieldName) => fieldName switch
    {
        "FullName"           => "Full Name",
        "IsMale"             => "Gender",
        "DateOfBirth"        => "Date of Birth",
        "BloodType"          => "Blood Type",
        "PatientCode"        => "Patient Code",
        "CountryGeoNameId"   => "Country",
        "StateGeoNameId"     => "State",
        "CityGeoNameId"      => "City",
        "IsActive"           => "Status",
        "IsRevoked"          => "Token Status",
        "ExpiryTime"         => "Expiry",
        "PhoneNumber"        => "Phone",
        "Role"               => "Role",
        "Name"               => "Name",
        "Email"              => "Email",
        "EVENT"              => "Event",
        _                    => System.Text.RegularExpressions.Regex.Replace(fieldName, "([A-Z])", " $1").Trim()
    };

    /// <summary>Converts raw EF values to human-readable strings for audit logs.</summary>
    private object? HumanizeValue(string fieldName, object? value)
    {
        if (value is null) return null;

        return fieldName switch
        {
            "IsMale"    => value is bool b ? (b ? "Male" : "Female") : value,
            "IsActive"  => value is bool a ? (a ? "Active" : "Inactive") : value,
            "IsDeleted" => value is bool d ? (d ? "Deleted" : "Active") : value,
            "IsRevoked" => value is bool r ? (r ? "Revoked" : "Active") : value,

            "BloodType" => value switch
            {
                BloodType bt => bt switch
                {
                    BloodType.APositive  => "A+",  BloodType.ANegative  => "A-",
                    BloodType.BPositive  => "B+",  BloodType.BNegative  => "B-",
                    BloodType.ABPositive => "AB+", BloodType.ABNegative => "AB-",
                    BloodType.OPositive  => "O+",  BloodType.ONegative  => "O-",
                    _ => bt.ToString()
                },
                int i => i switch
                {
                    1 => "A+", 2 => "A-", 3 => "B+", 4 => "B-",
                    5 => "AB+", 6 => "AB-", 7 => "O+", 8 => "O-",
                    _ => value
                },
                _ => value
            },

            "DateOfBirth" => value is DateTime dt
                ? dt.ToString("yyyy-MM-dd")
                : value,

            // Resolve GeoName IDs to names from cache
            "CountryGeoNameId" => ResolveLocationName("countries", value),
            "StateGeoNameId"   => ResolveLocationName(null, value),
            "CityGeoNameId"    => ResolveLocationName(null, value),

            // Skip internal IDs
            "ClinicId" or "UserId" or "PatientId" or "StaffId" or
            "CreatedBy" or "UpdatedBy" or "Id" => null,

            _ => value
        };
    }

    /// <summary>
    /// Resolves a GeoName ID to its English name using the in-memory cache.
    /// Falls back to the raw ID string if not cached yet.
    /// </summary>
    private object? ResolveLocationName(string? countryCacheKey, object? value)
    {
        if (value is null) return null;
        if (_cache is null) return value;

        int geoId;
        if (value is int i) geoId = i;
        else if (value is long l) geoId = (int)l;
        else return value;

        // Try countries cache
        if (countryCacheKey != null && _cache.TryGetValue(countryCacheKey, out List<GeoNamesCountry>? countries) && countries != null)
        {
            var match = countries.FirstOrDefault(c => c.GeonameId == geoId);
            if (match != null) return match.Name.En;
        }

        // Try all state/city caches — scan cached entries by known key patterns
        // States are cached as "states_{countryId}", cities as "cities_{stateId}"
        // We don't know the parent ID here, so we try a direct lookup by geoId
        if (_cache.TryGetValue($"_geoname_{geoId}", out string? cachedName) && cachedName != null)
            return cachedName;

        // Scan states caches — GeoNamesService caches by parent ID, not by child ID
        // As a fallback, return the ID as a string so it's at least visible
        return $"#{geoId}";
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
