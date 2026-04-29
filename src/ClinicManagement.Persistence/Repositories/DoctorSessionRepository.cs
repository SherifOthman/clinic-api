using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class DoctorSessionRepository : IDoctorSessionRepository
{
    private readonly DbSet<DoctorSession> _set;

    public DoctorSessionRepository(ApplicationDbContext ctx) => _set = ctx.Set<DoctorSession>();

    public Task<DoctorSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<DoctorSession?> GetByDoctorBranchDateAsync(Guid doctorInfoId, Guid branchId, DateOnly date, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(s => s.DoctorInfoId == doctorInfoId && s.BranchId == branchId && s.Date == date, ct);

    public Task<List<DoctorSession>> GetByBranchAndDateAsync(Guid branchId, DateOnly date, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Where(s => s.BranchId == branchId && s.Date == date)
               .ToListAsync(ct);

    public async Task AddAsync(DoctorSession session, CancellationToken ct = default)
        => await _set.AddAsync(session, ct);

    public void Update(DoctorSession session) => _set.Update(session);
}
