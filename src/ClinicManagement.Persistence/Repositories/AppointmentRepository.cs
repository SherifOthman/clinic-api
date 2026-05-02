using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Appointments.Queries;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly DbSet<Appointment> _set;

    public AppointmentRepository(ApplicationDbContext ctx) => _set = ctx.Set<Appointment>();

    // ── Projected reads — push mapping to SQL, avoid entity materialisation ──

    public Task<List<AppointmentDto>> GetProjectedByDoctorsAndDateAsync(
        List<Guid> doctorInfoIds, DateOnly date, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Where(a => doctorInfoIds.Contains(a.DoctorInfoId) && a.Date == date)
               .OrderBy(a => a.DoctorInfoId)
               .ThenBy(a => a.QueueNumber ?? 0)
               .ThenBy(a => a.ScheduledTime ?? TimeOnly.MinValue)
               .Select(a => new AppointmentDto(
                   a.Id,
                   a.DoctorInfoId,
                   a.Doctor != null ? a.Doctor.ClinicMember.User!.FullName : "—",
                   a.PatientId,
                   a.Patient != null ? a.Patient.FullName : "—",
                   a.Patient != null ? a.Patient.PatientCode : null,
                   a.QueueNumber,
                   a.ScheduledTime != null ? a.ScheduledTime.Value.ToString("HH:mm") : null,
                   a.EndTime != null ? a.EndTime.Value.ToString("HH:mm") : null,
                   a.VisitDurationMinutes,
                   a.Type.ToString(),
                   a.Status.ToString(),
                   a.VisitType != null ? a.VisitType.Name : "—",
                   a.FinalPrice,
                   a.CreatedAt,
                   a.Patient != null ? a.Patient.Gender.ToString() : null,
                   a.Patient != null ? a.Patient.DateOfBirth : null))
               .ToListAsync(ct);

    public Task<List<AppointmentDto>> GetProjectedByBranchAndDateAsync(
        Guid branchId, DateOnly date, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Where(a => a.BranchId == branchId && a.Date == date)
               .OrderBy(a => a.DoctorInfoId)
               .ThenBy(a => a.QueueNumber ?? 0)
               .ThenBy(a => a.ScheduledTime ?? TimeOnly.MinValue)
               .Select(a => new AppointmentDto(
                   a.Id,
                   a.DoctorInfoId,
                   a.Doctor != null ? a.Doctor.ClinicMember.User!.FullName : "—",
                   a.PatientId,
                   a.Patient != null ? a.Patient.FullName : "—",
                   a.Patient != null ? a.Patient.PatientCode : null,
                   a.QueueNumber,
                   a.ScheduledTime != null ? a.ScheduledTime.Value.ToString("HH:mm") : null,
                   a.EndTime != null ? a.EndTime.Value.ToString("HH:mm") : null,
                   a.VisitDurationMinutes,
                   a.Type.ToString(),
                   a.Status.ToString(),
                   a.VisitType != null ? a.VisitType.Name : "—",
                   a.FinalPrice,
                   a.CreatedAt,
                   a.Patient != null ? a.Patient.Gender.ToString() : null,
                   a.Patient != null ? a.Patient.DateOfBirth : null))
               .ToListAsync(ct);

    // ── Entity reads — AsNoTracking for performance ───────────────────────────

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Include(a => a.Patient)
               .Include(a => a.VisitType)
               .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<List<Appointment>> GetByDoctorAndDateAsync(Guid doctorInfoId, DateOnly date, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Include(a => a.Patient)
               .Include(a => a.VisitType)
               .Where(a => a.DoctorInfoId == doctorInfoId && a.Date == date)
               .OrderBy(a => a.QueueNumber ?? 0)
               .ThenBy(a => a.ScheduledTime ?? TimeOnly.MinValue)
               .ToListAsync(ct);

    public Task<List<Appointment>> GetByDoctorsAndDateAsync(List<Guid> doctorInfoIds, DateOnly date, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Include(a => a.Patient)
               .Include(a => a.VisitType)
               .Include(a => a.Doctor).ThenInclude(d => d.ClinicMember).ThenInclude(m => m.User)
               .Where(a => doctorInfoIds.Contains(a.DoctorInfoId) && a.Date == date)
               .OrderBy(a => a.DoctorInfoId)
               .ThenBy(a => a.QueueNumber ?? 0)
               .ThenBy(a => a.ScheduledTime ?? TimeOnly.MinValue)
               .ToListAsync(ct);

    public Task<List<Appointment>> GetByBranchAndDateAsync(Guid branchId, DateOnly date, CancellationToken ct = default)
        => _set.AsNoTracking()
               .Include(a => a.Patient)
               .Include(a => a.VisitType)
               .Include(a => a.Doctor).ThenInclude(d => d.ClinicMember).ThenInclude(m => m.User)
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
