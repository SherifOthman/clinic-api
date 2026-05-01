using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Auth.QueryModels;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbSet<User> _users;
    private readonly DbSet<IdentityUserRole<Guid>> _userRoles;
    private readonly DbSet<Role> _roles;
    private readonly DbSet<ClinicMember> _members;
    private readonly DbSet<Clinic> _clinics;

    public UserRepository(ApplicationDbContext context)
    {
        _users     = context.Set<User>();
        _userRoles = context.Set<IdentityUserRole<Guid>>();
        _roles     = context.Set<Role>();
        _members   = context.Set<ClinicMember>();
        _clinics   = context.Set<Clinic>();
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByIdWithPersonAsync(Guid id, CancellationToken ct = default)
        => await _users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername, CancellationToken ct = default)
        => await _users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.UserName == emailOrUsername, ct);

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
        var roles = await (
            from ur in _userRoles
            join r in _roles on ur.RoleId equals r.Id
            where ur.UserId == userId
            select r.Name!
        ).ToListAsync(ct);

        return roles.Select(r => new UserRoleRow(r)).ToList();
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
        var context = await _members
            .AsNoTracking()
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
