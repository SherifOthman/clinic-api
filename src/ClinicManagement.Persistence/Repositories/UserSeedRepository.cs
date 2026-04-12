using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

/// <summary>
/// Minimal repository for seeding User entities in tests.
/// User extends IdentityUser, not BaseEntity, so it can't use Repository&lt;T&gt;.
/// </summary>
public class UserSeedRepository : IUserSeedRepository
{
    private readonly DbSet<User> _users;

    public UserSeedRepository(ApplicationDbContext context)
    {
        _users = context.Set<User>();
    }

    public void Add(User user) => _users.Add(user);
}
