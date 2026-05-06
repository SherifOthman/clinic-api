using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>Lightweight projection used by UpdateAppointmentHandler to avoid EF tracking conflicts.</summary>
public record VisitTypePriceInfo(decimal Price, int? DefaultDoctorDurationMinutes);

/// <summary>
/// Replaces IWorkingDaysRepository + IDoctorVisitTypeRepository.
/// Works with DoctorBranchSchedule, WorkingDay, and VisitType.
/// </summary>
public interface IDoctorScheduleRepository
{
    // ── Schedule ──────────────────────────────────────────────────────────────

    Task<DoctorBranchSchedule?> GetScheduleAsync(Guid doctorInfoId, Guid branchId, CancellationToken ct = default);
    Task<DoctorBranchSchedule> GetOrCreateScheduleAsync(Guid doctorInfoId, Guid branchId, CancellationToken ct = default);

    /// <summary>Returns all active doctors (with DoctorInfo) for a given branch.</summary>
    Task<List<DoctorForBranchRow>> GetDoctorsForBranchAsync(Guid branchId, CancellationToken ct = default);

    // ── Working days ──────────────────────────────────────────────────────────

    Task<List<WorkingDayRow>> GetWorkingDaysByDoctorInfoIdAsync(Guid doctorInfoId, CancellationToken ct = default);
    Task<List<WorkingDay>> GetWorkingDayEntitiesAsync(Guid scheduleId, CancellationToken ct = default);
    void AddWorkingDay(WorkingDay day);
    void RemoveWorkingDays(IEnumerable<WorkingDay> days);

    // ── Visit types ───────────────────────────────────────────────────────────

    Task<List<VisitType>> GetVisitTypesByScheduleAsync(Guid scheduleId, CancellationToken ct = default);
    Task<VisitType?> GetVisitTypeByIdAsync(Guid visitTypeId, CancellationToken ct = default);

    /// <summary>
    /// Returns only the price and doctor's default duration for a visit type.
    /// Uses AsNoTracking — safe to call alongside tracked appointment entities.
    /// Returns null if the visit type doesn't exist or is inactive.
    /// </summary>
    Task<VisitTypePriceInfo?> GetVisitTypePriceAsync(Guid visitTypeId, CancellationToken ct = default);

    Task<bool> VisitTypeHasAppointmentsAsync(Guid visitTypeId, CancellationToken ct = default);
    void AddVisitType(VisitType visitType);
    void RemoveVisitType(VisitType visitType);
}
