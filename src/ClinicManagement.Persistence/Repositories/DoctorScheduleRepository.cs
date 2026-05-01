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

    public async Task<List<DoctorForBranchRow>> GetDoctorsForBranchAsync(Guid branchId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Load schedules and sessions in two flat queries — avoids a correlated
        // subquery per doctor that EF generates when .Any() is inside .Select()
        var schedules = await _db.Set<DoctorBranchSchedule>()
            .AsNoTracking()
            .Where(s => s.BranchId == branchId && s.IsActive)
            .Select(s => new
            {
                s.DoctorInfoId,
                MemberId        = s.DoctorInfo.ClinicMemberId,
                FullName        = s.DoctorInfo.ClinicMember.Person.FullName,
                ProfileImageUrl = s.DoctorInfo.ClinicMember.Person.ProfileImageUrl,
                AppointmentType = s.AppointmentType.ToString(),
                s.DoctorInfo.DefaultVisitDurationMinutes,
            })
            .ToListAsync(ct);

        if (schedules.Count == 0)
            return [];

        // Single query for all sessions today — no per-doctor subquery
        var doctorInfoIds = schedules.Select(s => s.DoctorInfoId).ToList();
        var checkedInToday = await _db.Set<DoctorSession>()
            .AsNoTracking()
            .Where(ds => doctorInfoIds.Contains(ds.DoctorInfoId) && ds.BranchId == branchId && ds.Date == today)
            .Select(ds => ds.DoctorInfoId)
            .ToHashSetAsync(ct);

        return schedules
            .Select(s => new DoctorForBranchRow(
                s.DoctorInfoId,
                s.MemberId,
                s.FullName,
                s.ProfileImageUrl,
                s.AppointmentType,
                s.DefaultVisitDurationMinutes,
                checkedInToday.Contains(s.DoctorInfoId)))
            .ToList();
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
