using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class DoctorScheduleRepository : IDoctorScheduleRepository
{
    private readonly ApplicationDbContext _db;

    public DoctorScheduleRepository(ApplicationDbContext db) => _db = db;

    // ── Schedule ──────────────────────────────────────────────────────────────

    public Task<DoctorBranchSchedule?> GetScheduleAsync(Guid doctorInfoId, Guid branchId, CancellationToken ct = default)
        => _db.Set<DoctorBranchSchedule>()
            .Include(s => s.WorkingDays)
            .Include(s => s.VisitTypes)
            .FirstOrDefaultAsync(s => s.DoctorInfoId == doctorInfoId && s.BranchId == branchId, ct);

    public async Task<DoctorBranchSchedule> GetOrCreateScheduleAsync(Guid doctorInfoId, Guid branchId, CancellationToken ct = default)
    {
        var existing = await GetScheduleAsync(doctorInfoId, branchId, ct);
        if (existing is not null) return existing;

        // Seed the appointment type from the doctor's clinic-wide default
        var doctorInfo = await _db.Set<DoctorInfo>().FindAsync([doctorInfoId], ct);
        var schedule = new DoctorBranchSchedule
        {
            DoctorInfoId    = doctorInfoId,
            BranchId        = branchId,
            AppointmentType = doctorInfo?.AppointmentType ?? Domain.Enums.AppointmentType.Queue,
        };
        _db.Set<DoctorBranchSchedule>().Add(schedule);
        return schedule;
    }

    public Task<List<DoctorForBranchRow>> GetDoctorsForBranchAsync(Guid branchId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return _db.Set<DoctorBranchSchedule>()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && s.IsActive)
            .Select(s => new DoctorForBranchRow(
                s.DoctorInfoId,
                s.DoctorInfo.ClinicMemberId,
                s.DoctorInfo.ClinicMember.Person.FullName,
                s.DoctorInfo.ClinicMember.Person.ProfileImageUrl,
                s.AppointmentType.ToString(),                          // ← per-branch now
                s.DoctorInfo.DefaultVisitDurationMinutes,
                _db.Set<DoctorSession>().Any(ds => ds.DoctorInfoId == s.DoctorInfoId && ds.BranchId == branchId && ds.Date == today)))
            .ToListAsync(ct);
    }

    public async Task<List<WorkingDayRow>> GetWorkingDaysByDoctorInfoIdAsync(Guid doctorInfoId, CancellationToken ct = default)
        => await _db.Set<WorkingDay>()
            .AsNoTracking()
            .Where(w => w.Schedule.DoctorInfoId == doctorInfoId)
            .OrderBy(w => w.Schedule.BranchId).ThenBy(w => w.Day)
            .Select(w => new WorkingDayRow(
                (int)w.Day,
                w.StartTime.ToString("HH:mm"),
                w.EndTime.ToString("HH:mm"),
                w.IsAvailable,
                w.Schedule.BranchId))
            .ToListAsync(ct);

    public Task<List<WorkingDay>> GetWorkingDayEntitiesAsync(Guid scheduleId, CancellationToken ct = default)
        => _db.Set<WorkingDay>().Where(w => w.DoctorBranchScheduleId == scheduleId).ToListAsync(ct);

    public void AddWorkingDay(WorkingDay day) => _db.Set<WorkingDay>().Add(day);
    public void RemoveWorkingDays(IEnumerable<WorkingDay> days) => _db.Set<WorkingDay>().RemoveRange(days);

    // ── Visit types ───────────────────────────────────────────────────────────

    public Task<List<VisitType>> GetVisitTypesByScheduleAsync(Guid scheduleId, CancellationToken ct = default)
        => _db.Set<VisitType>()
            .Where(v => v.DoctorBranchScheduleId == scheduleId)
            .OrderBy(v => v.Name)
            .ToListAsync(ct);

    public Task<VisitType?> GetVisitTypeByIdAsync(Guid visitTypeId, CancellationToken ct = default)
        => _db.Set<VisitType>().Include(v => v.Schedule).FirstOrDefaultAsync(v => v.Id == visitTypeId, ct);

    public Task<bool> VisitTypeHasAppointmentsAsync(Guid visitTypeId, CancellationToken ct = default)
        => _db.Set<Appointment>().AnyAsync(a => a.VisitTypeId == visitTypeId, ct);

    public void AddVisitType(VisitType visitType) => _db.Set<VisitType>().Add(visitType);
    public void RemoveVisitType(VisitType visitType) => _db.Set<VisitType>().Remove(visitType);
}
