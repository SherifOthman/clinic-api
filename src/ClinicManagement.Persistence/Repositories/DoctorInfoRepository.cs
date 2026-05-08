using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class DoctorInfoRepository : Repository<DoctorInfo>, IDoctorInfoRepository
{
    public DoctorInfoRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Guid> GetIdByMemberIdAsync(Guid clinicMemberId, CancellationToken ct = default)
    {
        var id = await DbSet
            .Where(d => d.ClinicMemberId == clinicMemberId)
            .Select(d => (Guid?)d.Id)
            .FirstOrDefaultAsync(ct);
        return id ?? Guid.Empty;
    }

    public new async Task<DoctorInfo?> GetByIdAsync(Guid doctorInfoId, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(d => d.Id == doctorInfoId, ct);
}
