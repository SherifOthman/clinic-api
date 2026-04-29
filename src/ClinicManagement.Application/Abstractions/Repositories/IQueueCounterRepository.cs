namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IQueueCounterRepository
{
    /// <summary>
    /// Atomically increments the queue counter for (doctorInfoId, date) and returns
    /// the next queue number. Creates the row if it doesn't exist yet.
    /// Uses SQL MERGE WITH (HOLDLOCK) — safe under concurrent receptionist inserts.
    /// </summary>
    Task<int> NextAsync(Guid doctorInfoId, DateOnly date, CancellationToken ct = default);
}
