using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Core DbSets
    DbSet<User> Users { get; }
    DbSet<Clinic> Clinics { get; }
    DbSet<ClinicBranch> ClinicBranches { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Receptionist> Receptionists { get; }
    DbSet<Patient> Patients { get; }
    DbSet<Appointment> Appointments { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
