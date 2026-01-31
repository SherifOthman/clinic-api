using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Patient> Patients { get; }
    DbSet<PatientPhoneNumber> PatientPhoneNumbers { get; }
    DbSet<Clinic> Clinics { get; }
    DbSet<ClinicBranch> ClinicBranches { get; }
    DbSet<ClinicBranchPhoneNumber> ClinicBranchPhoneNumbers { get; }
    DbSet<Specialization> Specializations { get; }
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<ChronicDisease> ChronicDiseases { get; }
    DbSet<PatientChronicDisease> PatientChronicDiseases { get; }
    DbSet<RateLimitEntry> RateLimitEntries { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<UserClinic> UserClinics { get; }
    
    DatabaseFacade Database { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
