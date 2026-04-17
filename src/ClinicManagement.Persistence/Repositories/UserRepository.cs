using ClinicManagement.Application.Features.Auth.QueryModels;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DbSet<User>                   _users;
    private readonly DbSet<IdentityUserRole<Guid>> _userRoles;
    private readonly DbSet<Role>                   _roles;
    private readonly DbSet<ClinicMember>           _members;
    private readonly DbSet<Clinic>                 _clinics;

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

    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername, CancellationToken ct = default)
        => await _users.FirstOrDefaultAsync(
            u => u.Email == emailOrUsername || u.UserName == emailOrUsername, ct);

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

    public async Task<UserSpecializationRow?> GetDoctorSpecializationAsync(Guid userId, CancellationToken ct = default)
    {
        var spec = await _members
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.DoctorInfo != null && m.DoctorInfo.Specialization != null)
            .Select(m => new
            {
                m.DoctorInfo!.Specialization!.NameEn,
                m.DoctorInfo!.Specialization!.NameAr,
            })
            .FirstOrDefaultAsync(ct);

        return spec is null ? null : new UserSpecializationRow(spec.NameEn, spec.NameAr);
    }

    public async Task<bool> HasClinicAsync(Guid userId, CancellationToken ct = default)
        => await _clinics
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AnyAsync(c => c.OwnerUserId == userId, ct);

    public async Task<UserProfileRow?> GetProfileAsync(Guid userId, CancellationToken ct = default)
        => await _users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileRow(
                u.UserName!, u.FirstName, u.LastName, u.Email!,
                u.PhoneNumber, u.ProfileImageUrl, u.EmailConfirmed, u.Gender.ToString()))
            .FirstOrDefaultAsync(ct);
}
