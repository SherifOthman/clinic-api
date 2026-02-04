using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class PatientChronicDiseaseRepository : BaseRepository<PatientChronicDisease>, IPatientChronicDiseaseRepository
{
    public PatientChronicDiseaseRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PatientChronicDisease>> GetByPatientIdAsync(Guid PatientId, CancellationToken cancellationToken = default)
    {
        return await _context.PatientChronicDiseases
            .Include(pcd => pcd.ChronicDisease)
            .Where(pcd => pcd.PatientId == PatientId)
            .OrderBy(pcd => pcd.ChronicDisease.NameEn)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PatientChronicDisease>> GetActiveByPatientIdAsync(Guid PatientId, CancellationToken cancellationToken = default)
    {
        // Since the current entity doesn't have IsActive, return all
        return await GetByPatientIdAsync(PatientId, cancellationToken);
    }

    public async Task<PatientChronicDisease?> GetByPatientAndDiseaseAsync(Guid PatientId, Guid chronicDiseaseId, CancellationToken cancellationToken = default)
    {
        return await _context.PatientChronicDiseases
            .Include(pcd => pcd.ChronicDisease)
            .Include(pcd => pcd.Patient)
            .FirstOrDefaultAsync(pcd => pcd.PatientId == PatientId && pcd.ChronicDiseaseId == chronicDiseaseId, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid PatientId, Guid chronicDiseaseId, CancellationToken cancellationToken = default)
    {
        return await _context.PatientChronicDiseases
            .AnyAsync(pcd => pcd.PatientId == PatientId && pcd.ChronicDiseaseId == chronicDiseaseId, cancellationToken);
    }

    public async Task<IEnumerable<PatientChronicDisease>> GetByChronicDiseaseIdAsync(Guid chronicDiseaseId, CancellationToken cancellationToken = default)
    {
        return await _context.PatientChronicDiseases
            .Include(pcd => pcd.Patient)
            .Where(pcd => pcd.ChronicDiseaseId == chronicDiseaseId)
            .OrderBy(pcd => $"{pcd.Patient.FirstName} {pcd.Patient.LastName}")
            .ToListAsync(cancellationToken);
    }
}
