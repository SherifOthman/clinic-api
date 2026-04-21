namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks the next patient code sequence per clinic.
/// Acts like an identity column scoped to each clinic.
///
/// PatientCode format: zero-padded 4-digit number, e.g. "0001", "0002"
/// Each clinic starts at 1 and increments independently.
/// Supports up to 9,999 patients per clinic.
/// </summary>
public class PatientCounter
{
    public Guid ClinicId { get; set; }

    /// <summary>The last issued patient code number. Incremented atomically on each patient creation.</summary>
    public int LastValue { get; set; } = 0;
}
