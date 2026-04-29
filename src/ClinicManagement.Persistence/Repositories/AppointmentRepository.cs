using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly DbSet<Appointment> _set;

    public AppointmentRepository(ApplicationDbContext ctx) => _set = ctx.Set<Appointment>();

    // ── Reads — AsNoTracking for performance ──────────────────────────────────

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Include(a => a.Patient).ThenInclude(p => p.Person)
               .Include(a => a.VisitType)
               .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<List<Appointment>> GetByDoctorAndDateAsync(Guid doctorInfoId, DateOnly date, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Include(a => a.Patient).ThenInclude(p => p.Person)
               .Include(a => a.VisitType)
               .Where(a => a.DoctorInfoId == doctorInfoId && a.Date == date)
               .OrderBy(a => a.QueueNumber ?? 0)
               .ThenBy(a => a.ScheduledTime ?? TimeOnly.MinValue)
               .ToListAsync(ct);

    public Task<List<Appointment>> GetByDoctorsAndDateAsync(List<Guid> doctorInfoIds, DateOnly date, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Include(a => a.Patient).ThenInclude(p => p.Person)
               .Include(a => a.VisitType)
               .Where(a => doctorInfoIds.Contains(a.DoctorInfoId) && a.Date == date)
               .OrderBy(a => a.DoctorInfoId)
               .ThenBy(a => a.QueueNumber ?? 0)
               .ThenBy(a => a.ScheduledTime ?? TimeOnly.MinValue)
               .ToListAsync(ct);

    public Task<List<Appointment>> GetByBranchAndDateAsync(Guid branchId, DateOnly date, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Include(a => a.Patient).ThenInclude(p => p.Person)
               .Include(a => a.VisitType)
               .Include(a => a.Doctor).ThenInclude(d => d.ClinicMember).ThenInclude(m => m.Person)
               .Where(a => a.BranchId == branchId && a.Date == date)
               .OrderBy(a => a.DoctorInfoId)
               .ThenBy(a => a.QueueNumber ?? 0)
               .ThenBy(a => a.ScheduledTime ?? TimeOnly.MinValue)
               .ToListAsync(ct);

    public Task<bool> TimeSlotTakenAsync(Guid doctorInfoId, DateOnly date, TimeOnly time, Guid? excludeId, CancellationToken ct = default)
        => _set.AsNoTracking()
               .AnyAsync(a =>
                   a.DoctorInfoId == doctorInfoId &&
                   a.Date == date &&
                   a.ScheduledTime == time &&
                   (excludeId == null || a.Id != excludeId), ct);

    // ── Writes ────────────────────────────────────────────────────────────────

    public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
        => await _set.AddAsync(appointment, ct);

    public void Update(Appointment appointment) => _set.Update(appointment);

    /// <summary>Tracked load for mutations — no AsNoTracking.</summary>
    public Task<Appointment?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(a => a.Id == id, ct);
}
