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
        => await _users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new GetMeProjection(
                u.UserName!,
                u.Person.FullName,
                u.Email!,
                u.PhoneNumber,
                u.Person.ProfileImageUrl,
                u.EmailConfirmed,
                u.Person.Gender.ToString(),
                u.PasswordHash != null,
                // OnboardingCompleted = has an owned clinic
                _clinics.Any(c => c.OwnerUserId == userId),
                // Member fields — null when user has no clinic membership (e.g. super admin)
                _members.Where(m => m.UserId == userId).Select(m => (Guid?)m.Id).FirstOrDefault(),
                _members.Where(m => m.UserId == userId && m.DoctorInfo != null)
                        .Select(m => (Guid?)m.DoctorInfo!.Id).FirstOrDefault(),
                _members.Where(m => m.UserId == userId && m.DoctorInfo != null && m.DoctorInfo.Specialization != null)
                        .Select(m => m.DoctorInfo!.Specialization!.NameEn).FirstOrDefault(),
                _members.Where(m => m.UserId == userId && m.DoctorInfo != null && m.DoctorInfo.Specialization != null)
                        .Select(m => m.DoctorInfo!.Specialization!.NameAr).FirstOrDefault(),
                // WeekStartDay — from owned clinic or from member's clinic
                _clinics.Where(c => c.OwnerUserId == userId).Select(c => (int?)c.WeekStartDay).FirstOrDefault()
                ?? _members.Where(m => m.UserId == userId)
                           .Join(_clinics, m => m.ClinicId, c => c.Id, (m, c) => (int?)c.WeekStartDay)
                           .FirstOrDefault()
                ?? 6,
                u.LastLoginAt,
                u.LastPasswordChangeAt))
            .FirstOrDefaultAsync(ct);
}
