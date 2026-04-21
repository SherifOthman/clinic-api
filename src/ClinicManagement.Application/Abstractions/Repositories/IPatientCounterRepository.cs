namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IPatientCounterRepository
{
    /// <summary>
    /// Atomically increments the patient counter for the given clinic and returns
    /// the next patient code as a plain integer.
    /// Creates the counter row if it doesn't exist yet (first patient in clinic).
    /// Uses a pessimistic lock to prevent duplicate codes under concurrent inserts.
    /// </summary>
    Task<string> NextCodeAsync(Guid clinicId, CancellationToken ct = default);
}
