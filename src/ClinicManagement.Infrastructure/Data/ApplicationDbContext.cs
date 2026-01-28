using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientPhoneNumber> PatientPhoneNumbers => Set<PatientPhoneNumber>();
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicBranch> ClinicBranches => Set<ClinicBranch>();
    public DbSet<ClinicBranchPhoneNumber> ClinicBranchPhoneNumbers => Set<ClinicBranchPhoneNumber>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<ChronicDisease> ChronicDiseases => Set<ChronicDisease>();
    public DbSet<PatientChronicDisease> PatientChronicDiseases => Set<PatientChronicDisease>();
    public DbSet<RateLimitEntry> RateLimitEntries => Set<RateLimitEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

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
        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure EF Core 10 Named Query Filters for Multi-Tenancy
        // Named filters allow selective disabling of specific filters while keeping others active
        ConfigureQueryFilters(builder);
    }

    private void ConfigureQueryFilters(ModelBuilder builder)
    {
        // Patient entity - both soft delete and tenant filtering
        builder.Entity<Patient>()
            .HasQueryFilter("SoftDeleteFilter", p => !p.IsDeleted)
            .HasQueryFilter("TenantFilter", p => p.ClinicId == _currentUserService.ClinicId);

        // ClinicBranch entity - tenant filtering
        builder.Entity<ClinicBranch>()
            .HasQueryFilter("TenantFilter", cb => cb.ClinicId == _currentUserService.ClinicId);

        // ClinicBranchPhoneNumber entity - tenant filtering through ClinicBranch
        builder.Entity<ClinicBranchPhoneNumber>()
            .HasQueryFilter("TenantFilter", cbp => cbp.ClinicBranch.ClinicId == _currentUserService.ClinicId);

        // PatientPhoneNumber entity - tenant filtering through Patient
        builder.Entity<PatientPhoneNumber>()
            .HasQueryFilter("TenantFilter", pp => pp.Patient.ClinicId == _currentUserService.ClinicId);

        // PatientChronicDisease entity - tenant filtering through Patient
        builder.Entity<PatientChronicDisease>()
            .HasQueryFilter("TenantFilter", pcd => pcd.Patient.ClinicId == _currentUserService.ClinicId);
    }
}