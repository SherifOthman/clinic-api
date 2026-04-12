using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>
/// Minimal repository for seeding User entities in tests.
/// User extends IdentityUser, not BaseEntity, so it can't use IRepository&lt;T&gt;.
/// </summary>
public interface IUserSeedRepository
{
    void Add(User user);
}
