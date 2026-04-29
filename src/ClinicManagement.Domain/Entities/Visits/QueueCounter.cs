namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks the next queue number per doctor per day.
/// Keyed by (DoctorInfoId, Date) — one row per doctor per working day.
///
/// Uses SQL MERGE WITH (HOLDLOCK) for atomic increment under concurrency —
/// same pattern as PatientCounter. Prevents duplicate queue numbers when
/// multiple receptionists book simultaneously.
/// </summary>
public class QueueCounter
{
    public Guid DoctorInfoId { get; set; }
    public DateOnly Date { get; set; }

    /// <summary>Last issued queue number. Starts at 0; first patient gets 1.</summary>
    public int LastValue { get; set; } = 0;
}
