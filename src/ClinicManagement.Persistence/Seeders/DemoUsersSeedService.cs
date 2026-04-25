using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Persistence.Seeders;

public class DemoUsersSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DemoUsersSeedService> _logger;
    private readonly SeedOptions _options;

    public DemoUsersSeedService(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DemoUsersSeedService> logger,
        IOptions<SeedOptions> options)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        if (!_options.SeedDemoUsers)
        {
            _logger.LogInformation("Demo user seeding is disabled");
            return;
        }

        await SeedSuperAdminAsync();
        var clinic = await SeedClinicOwnerAsync();
        if (clinic is null) return;
        await SeedDoctorAsync(clinic);
        await SeedReceptionistAsync(clinic);
    }

    private async Task SeedSuperAdminAsync()
    {
        var opts = _options.SuperAdmin;
        if (await _userManager.FindByEmailAsync(opts.Email) != null) return;

        var person = new Person { FullName = "System Administrator", Gender = Gender.Male };
        var user = new User
        {
            UserName = "superadmin",
            Email = opts.Email,
            PhoneNumber = "+966500000000",
            EmailConfirmed = true,
            PersonId = person.Id,
            Person = person,
        };
        var result = await _userManager.CreateAsync(user, opts.Password);
        if (!result.Succeeded) { LogError("SuperAdmin", result); return; }
        await _userManager.AddToRoleAsync(user, UserRoles.SuperAdmin);
        _logger.LogInformation("SuperAdmin seeded: {Email}", opts.Email);
    }

    private async Task<Clinic?> SeedClinicOwnerAsync()
    {
        var opts = _options.ClinicOwner;
        var owner = await _userManager.FindByEmailAsync(opts.Email);

        if (owner == null)
        {
            var person = new Person { FullName = "John Smith", Gender = Gender.Male };
            owner = new User
            {
                UserName = "owner",
                Email = opts.Email,
                PhoneNumber = "+1234567890",
                EmailConfirmed = true,
                PersonId = person.Id,
                Person = person,
            };
            var result = await _userManager.CreateAsync(owner, opts.Password);
            if (!result.Succeeded) { LogError("ClinicOwner", result); return null; }
            await _userManager.AddToRoleAsync(owner, UserRoles.ClinicOwner);
            await _userManager.AddToRoleAsync(owner, UserRoles.Doctor);
            _logger.LogInformation("ClinicOwner seeded: {Email}", opts.Email);
        }

        var existing = await _context.Set<Clinic>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(c => c.OwnerUserId == owner.Id);
        if (existing != null) return existing;

        var basicPlan = await _context.Set<SubscriptionPlan>().FirstOrDefaultAsync(p => p.Name == "Basic");
        if (basicPlan == null) { _logger.LogError("Basic plan not found"); return null; }

        var generalPractice = await _context.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "General Practice");

        var clinic = new Clinic
        {
            Name = "Demo Medical Clinic",
            OwnerUserId = owner.Id,
            SubscriptionPlanId = basicPlan.Id,
            OnboardingCompleted = true,
            IsActive = true,
            SubscriptionStartDate = DateTimeOffset.UtcNow,
            SubscriptionEndDate = DateTimeOffset.UtcNow.AddMonths(1),
            TrialEndDate = DateTimeOffset.UtcNow.AddDays(14),
        };
        _context.Set<Clinic>().Add(clinic);

        _context.Set<ClinicBranch>().Add(new ClinicBranch
        {
            ClinicId = clinic.Id,
            Name = "Main Branch",
            AddressLine = "123 Medical Street, Downtown",
            StateGeonameId = 360630,
            CityGeonameId = 360630,
            IsMainBranch = true,
            IsActive = true,
        });

        _context.Set<ClinicSubscription>().Add(new ClinicSubscription
        {
            ClinicId = clinic.Id,
            SubscriptionPlanId = basicPlan.Id,
            Status = SubscriptionStatus.Trial,
            StartDate = DateTimeOffset.UtcNow,
            TrialEndDate = DateTimeOffset.UtcNow.AddDays(14),
            AutoRenew = true,
        });

        var ownerMember = new ClinicMember
        {
            PersonId = owner.PersonId,
            UserId = owner.Id,
            ClinicId = clinic.Id,
            Role = ClinicMemberRole.Owner,
            IsActive = true,
        };
        _context.Set<ClinicMember>().Add(ownerMember);
        _context.Set<DoctorInfo>().Add(new DoctorInfo
        {
            ClinicMemberId = ownerMember.Id,
            SpecializationId = generalPractice?.Id,
        });

        // Seed default permissions for owner
        var ownerPermissions = DefaultPermissions.ForRole(ClinicMemberRole.Owner)
            .Select(p => new ClinicMemberPermission { ClinicMemberId = ownerMember.Id, Permission = p });
        _context.Set<ClinicMemberPermission>().AddRange(ownerPermissions);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Demo clinic seeded (ClinicId: {Id})", clinic.Id);
        return clinic;
    }

    private async Task SeedDoctorAsync(Clinic clinic)
    {
        var opts = _options.Doctor;
        var doctor = await _userManager.FindByEmailAsync(opts.Email);

        if (doctor == null)
        {
            var person = new Person { FullName = "Sarah Johnson", Gender = Gender.Female };
            doctor = new User
            {
                UserName = "doctor",
                Email = opts.Email,
                PhoneNumber = "+1234567891",
                EmailConfirmed = true,
                PersonId = person.Id,
                Person = person,
            };
            var result = await _userManager.CreateAsync(doctor, opts.Password);
            if (!result.Succeeded) { LogError("Doctor", result); return; }
            await _userManager.AddToRoleAsync(doctor, UserRoles.Doctor);
            _logger.LogInformation("Doctor seeded: {Email}", opts.Email);
        }

        var alreadyMember = await _context.Set<ClinicMember>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AnyAsync(m => m.UserId == doctor.Id && m.ClinicId == clinic.Id);
        if (alreadyMember) return;

        var cardiology = await _context.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "Cardiology");
        var member = new ClinicMember
        {
            PersonId = doctor.PersonId,
            UserId = doctor.Id,
            ClinicId = clinic.Id,
            Role = ClinicMemberRole.Doctor,
            IsActive = true,
        };
        _context.Set<ClinicMember>().Add(member);
        _context.Set<DoctorInfo>().Add(new DoctorInfo { ClinicMemberId = member.Id, SpecializationId = cardiology?.Id });

        var doctorPermissions = DefaultPermissions.ForRole(ClinicMemberRole.Doctor)
            .Select(p => new ClinicMemberPermission { ClinicMemberId = member.Id, Permission = p });
        _context.Set<ClinicMemberPermission>().AddRange(doctorPermissions);

        await _context.SaveChangesAsync();
    }

    private async Task SeedReceptionistAsync(Clinic clinic)
    {
        var opts = _options.Receptionist;
        var receptionist = await _userManager.FindByEmailAsync(opts.Email);

        if (receptionist == null)
        {
            var person = new Person { FullName = "Emily Davis", Gender = Gender.Female };
            receptionist = new User
            {
                UserName = "receptionist",
                Email = opts.Email,
                PhoneNumber = "+1234567892",
                EmailConfirmed = true,
                PersonId = person.Id,
                Person = person,
            };
            var result = await _userManager.CreateAsync(receptionist, opts.Password);
            if (!result.Succeeded) { LogError("Receptionist", result); return; }
            await _userManager.AddToRoleAsync(receptionist, UserRoles.Receptionist);
            _logger.LogInformation("Receptionist seeded: {Email}", opts.Email);
        }

        var alreadyMember = await _context.Set<ClinicMember>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AnyAsync(m => m.UserId == receptionist.Id && m.ClinicId == clinic.Id);
        if (alreadyMember) return;

        var receptionistMember = new ClinicMember
        {
            PersonId = receptionist.PersonId,
            UserId = receptionist.Id,
            ClinicId = clinic.Id,
            Role = ClinicMemberRole.Receptionist,
            IsActive = true,
        };
        _context.Set<ClinicMember>().Add(receptionistMember);

        var receptionistPermissions = DefaultPermissions.ForRole(ClinicMemberRole.Receptionist)
            .Select(p => new ClinicMemberPermission { ClinicMemberId = receptionistMember.Id, Permission = p });
        _context.Set<ClinicMemberPermission>().AddRange(receptionistPermissions);

        await _context.SaveChangesAsync();
    }

    private void LogError(string role, IdentityResult result) =>
        _logger.LogError("Failed to create {Role}: {Errors}", role, string.Join(", ", result.Errors.Select(e => e.Description)));
}
