using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicPatientChronicDiseaseRepository : BaseRepository<ClinicPatientChronicDisease>, IClinicPatientChronicDiseaseRepository
{
    public ClinicPatientChronicDiseaseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClinicPatientChronicDisease>> GetByClinicPatientIdAsync(Guid clinicPatientId, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicPatientChronicDiseases
            .Include(cpcd => cpcd.ChronicDisease)
            .Where(cpcd => cpcd.ClinicPatientId == clinicPatientId)
            .OrderBy(cpcd => cpcd.ChronicDisease.NameEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ClinicPatientChronicDisease>> GetActiveByClinicPatientIdAsync(Guid clinicPatientId, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicPatientChronicDiseases
            .Include(cpcd => cpcd.ChronicDisease)
            .Where(cpcd => cpcd.ClinicPatientId == clinicPatientId && cpcd.IsActive)
            .OrderBy(cpcd => cpcd.ChronicDisease.NameEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<ClinicPatientChronicDisease?> GetByClinicPatientAndDiseaseAsync(Guid clinicPatientId, Guid chronicDiseaseId, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicPatientChronicDiseases
            .Include(cpcd => cpcd.ChronicDisease)
            .Include(cpcd => cpcd.ClinicPatient)
            .FirstOrDefaultAsync(cpcd => cpcd.ClinicPatientId == clinicPatientId && cpcd.ChronicDiseaseId == chronicDiseaseId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid clinicPatientId, Guid chronicDiseaseId, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicPatientChronicDiseases
            .AnyAsync(cpcd => cpcd.ClinicPatientId == clinicPatientId && cpcd.ChronicDiseaseId == chronicDiseaseId, cancellationToken);
    }

    public async Task<IEnumerable<ClinicPatientChronicDisease>> GetByChronicDiseaseIdAsync(Guid chronicDiseaseId, CancellationToken cancellationToken = default)
    {
        return await _context.ClinicPatientChronicDiseases
            .Include(cpcd => cpcd.ClinicPatient)
            .Where(cpcd => cpcd.ChronicDiseaseId == chronicDiseaseId)
            .OrderBy(cpcd => cpcd.ClinicPatient.FullName)
            .ToListAsync(cancellationToken);
    }
}
