using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Essential DbSets for Auth and Staff Inviting only
    DbSet<User> Users { get; }
    DbSet<Clinic> Clinics { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Receptionist> Receptionists { get; }
    DbSet<Specialization> Specializations { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
