using ClinicManagement.Application.Features.Appointments.Queries;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IAppointmentRepository
{
    // ── Projected reads (preferred — no entity materialisation) ──────────────
    Task<List<AppointmentDto>> GetProjectedByDoctorsAndDateAsync(List<Guid> doctorInfoIds, DateOnly date, CancellationToken ct = default);
    Task<List<AppointmentDto>> GetProjectedByBranchAndDateAsync(Guid branchId, DateOnly date, CancellationToken ct = default);

    // ── Entity reads (AsNoTracking) — kept for detail/update scenarios ────────
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Appointment>> GetByDoctorAndDateAsync(Guid doctorInfoId, DateOnly date, CancellationToken ct = default);
    Task<List<Appointment>> GetByDoctorsAndDateAsync(List<Guid> doctorInfoIds, DateOnly date, CancellationToken ct = default);
    Task<List<Appointment>> GetByBranchAndDateAsync(Guid branchId, DateOnly date, CancellationToken ct = default);
    Task<bool> TimeSlotTakenAsync(Guid doctorInfoId, DateOnly date, TimeOnly time, Guid? excludeId, CancellationToken ct = default);

    // ── Writes ────────────────────────────────────────────────────────────────
    Task AddAsync(Appointment appointment, CancellationToken ct = default);

    /// <summary>Attach a tracked entity for update — call after GetByIdForUpdateAsync.</summary>
    void Update(Appointment appointment);

    /// <summary>Loads a tracked entity for mutation (no AsNoTracking).</summary>
    Task<Appointment?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
}
