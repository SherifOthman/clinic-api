using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MedicalFileRepository : BaseRepository<MedicalFile>, IMedicalFileRepository
{
    public MedicalFileRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<int> GetCountForClinicByYearAsync(Guid clinicId, int year, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(mf => mf.Patient.ClinicId == clinicId && mf.UploadedAt.Year == year)
            .CountAsync(cancellationToken);
    }
}
