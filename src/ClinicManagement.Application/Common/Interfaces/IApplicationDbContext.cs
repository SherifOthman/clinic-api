using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<ChronicDisease> ChronicDiseases { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Specialization> Specializations { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<ClinicPatientChronicDisease> ClinicPatientChronicDiseases { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<ClinicBranchAppointmentPrice> ClinicBranchAppointmentPrices { get; }
    DbSet<AppointmentType> AppointmentTypes { get; }
    
    DatabaseFacade Database { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
