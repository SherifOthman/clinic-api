using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class DoctorRepository : BaseRepository<Doctor>, IDoctorRepository
{
    public DoctorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Doctor>> GetDoctorsBySpecializationAsync(Guid specializationId, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Include(d => d.Specialization)
            .Where(d => d.SpecializationId == specializationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Doctor?> GetDoctorWithSpecializationAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Include(d => d.Specialization)
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);
    }
}
