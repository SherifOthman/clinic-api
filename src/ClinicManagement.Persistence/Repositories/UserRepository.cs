using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Auth.QueryModels;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbSet<User> _users;
    private readonly DbSet<IdentityUserRole<Guid>> _userRoles;
    private readonly DbSet<Role> _roles;
    private readonly DbSet<ClinicMember> _members;
    private readonly DbSet<Clinic> _clinics;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan RoleCacheDuration = TimeSpan.FromMinutes(30);
    private static string RoleCacheKey(Guid userId) => $"roles:{userId}";

    public UserRepository(ApplicationDbContext context, IMemoryCache cache)
    {
        _users     = context.Set<User>();
        _userRoles = context.Set<IdentityUserRole<Guid>>();
        _roles     = context.Set<Role>();
        _members   = context.Set<ClinicMember>();
        _clinics   = context.Set<Clinic>();
        _cache     = cache;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByIdWithPersonAsync(Guid id, CancellationToken ct = default)
        => await _users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername, CancellationToken ct = default)
        => await _users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.UserName == emailOrUsername, ct);

    public async Task<UserWithRoles?> GetByEmailOrUsernameWithRolesAsync(string emailOrUsername, CancellationToken ct = default)
    {
        var user = await _users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.UserName == emailOrUsername, ct);

        if (user is null) return null;

        var roles = await GetRolesCachedAsync(user.Id, ct);
        return new UserWithRoles(user, roles);
    }

    public async Task<bool> AnyByEmailAsync(string email, CancellationToken ct = default)
        => await _users.AnyAsync(u => u.Email == email, ct);

    public async Task<bool> AnyByUsernameAsync(string username, CancellationToken ct = default)
        => await _users.AnyAsync(u => u.UserName == username, ct);

    public async Task<bool> AnyByNormalizedEmailAsync(string normalizedEmail, CancellationToken ct = default)
        => await _users.AnyAsync(u => u.NormalizedEmail == normalizedEmail, ct);

    public async Task<bool> AnyByNormalizedUsernameAsync(string normalizedUsername, CancellationToken ct = default)
        => await _users.AnyAsync(u => u.NormalizedUserName == normalizedUsername, ct);

    public async Task<List<UserRoleRow>> GetRolesByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var roles = await GetRolesCachedAsync(userId, ct);
        return roles.Select(r => new UserRoleRow(r)).ToList();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Loads roles from cache or DB. Roles rarely change so 30-minute cache is safe.
    /// Cache is invalidated on role assignment (handled by callers that modify roles).
    /// </summary>
    private async Task<List<string>> GetRolesCachedAsync(Guid userId, CancellationToken ct)
    {
        if (_cache.TryGetValue(RoleCacheKey(userId), out List<string>? cached))
            return cached!;

        var roles = await (
            from ur in _userRoles
            join r in _roles on ur.RoleId equals r.Id
            where ur.UserId == userId
            select r.Name!
        ).ToListAsync(ct);

        _cache.Set(RoleCacheKey(userId), roles, RoleCacheDuration);
        return roles;
    }

    public async Task<GetMeProjection?> GetMeProjectionAsync(Guid userId, CancellationToken ct = default)
    {
        // Query 1: user profile — always a single row, no joins needed
        var profile = await _users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.UserName,
                u.Email,
                u.PhoneNumber,
                u.Person.FullName,
                u.Person.ProfileImageUrl,
                Gender         = u.Person.Gender.ToString(),
                u.EmailConfirmed,
                HasPassword    = u.PasswordHash != null,
                u.LastLoginAt,
                u.LastPasswordChangeAt,
            })
            .FirstOrDefaultAsync(ct);

        if (profile is null) return null;

        // Query 2: member + clinic context — one join covers all roles.
        // SuperAdmin has no member record → returns null → all context fields stay null.
        // IgnoreQueryFilters: we filter by UserId directly — tenant filter not needed here
        // and would throw when ClinicId is null (e.g. owner before onboarding completes).
        var context = await _members
            .AsNoTracking()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Where(m => m.UserId == userId)
            .Select(m => new
            {
                MemberId         = m.Id,
                DoctorInfoId     = m.DoctorInfo != null ? (Guid?)m.DoctorInfo.Id : null,
                SpecNameEn       = m.DoctorInfo != null && m.DoctorInfo.Specialization != null
                                     ? m.DoctorInfo.Specialization.NameEn : null,
                SpecNameAr       = m.DoctorInfo != null && m.DoctorInfo.Specialization != null
                                     ? m.DoctorInfo.Specialization.NameAr : null,
                m.Clinic.WeekStartDay,
                // OnboardingCompleted: true when the clinic owner has finished setup
                OnboardingCompleted = m.Clinic.OnboardingCompleted,
            })
            .FirstOrDefaultAsync(ct);

        // For clinic owners who haven't joined as a member yet (edge case during onboarding),
        // fall back to the owned clinic for WeekStartDay and OnboardingCompleted.
        int weekStartDay        = context?.WeekStartDay ?? 6;
        bool onboardingCompleted = context?.OnboardingCompleted ?? false;

        if (context is null)
        {
            var ownedClinic = await _clinics
                .AsNoTracking()
                .IgnoreQueryFilters([QueryFilterNames.Tenant])
                .Where(c => c.OwnerUserId == userId)
                .Select(c => new { c.WeekStartDay, c.OnboardingCompleted })
                .FirstOrDefaultAsync(ct);

            weekStartDay         = ownedClinic?.WeekStartDay ?? 6;
            onboardingCompleted  = ownedClinic?.OnboardingCompleted ?? false;
        }

        return new GetMeProjection(
            UserName:             profile.UserName!,
            FullName:             profile.FullName,
            Email:                profile.Email!,
            PhoneNumber:          profile.PhoneNumber,
            ProfileImageUrl:      profile.ProfileImageUrl,
            EmailConfirmed:       profile.EmailConfirmed,
            Gender:               profile.Gender,
            HasPassword:          profile.HasPassword,
            OnboardingCompleted:  onboardingCompleted,
            MemberId:             context?.MemberId,
            DoctorInfoId:         context?.DoctorInfoId,
            SpecializationNameEn: context?.SpecNameEn,
            SpecializationNameAr: context?.SpecNameAr,
            WeekStartDay:         weekStartDay,
            LastLoginAt:          profile.LastLoginAt,
            LastPasswordChangeAt: profile.LastPasswordChangeAt
        );
    }
}
