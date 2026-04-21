namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Tracks the next patient code sequence per clinic.
/// Acts like an identity column scoped to each clinic.
///
/// PatientCode format: zero-padded 7-digit number, e.g. "0000001", "0000002"
/// Each clinic starts at 1 and increments independently.
/// </summary>
public class PatientCounter
{
    public Guid ClinicId { get; set; }

    /// <summary>The last issued patient code number. Incremented atomically on each patient creation.</summary>
    public int LastValue { get; set; } = 0;
}
