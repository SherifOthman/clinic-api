using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class DoctorProfileRepository : Repository<Doctor>, IDoctorProfileRepository
{
    public DoctorProfileRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Guid> GetIdByStaffIdAsync(Guid staffId, CancellationToken ct = default)
        => await DbSet
            .Where(dp => dp.StaffId == staffId)
            .Select(dp => dp.Id)
            .FirstOrDefaultAsync(ct);
}
