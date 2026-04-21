using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class PatientCounterRepository : IPatientCounterRepository
{
    private readonly ApplicationDbContext _db;

    public PatientCounterRepository(ApplicationDbContext db) => _db = db;

    public async Task<string> NextCodeAsync(Guid clinicId, CancellationToken ct = default)
    {
        // Use a raw SQL UPDATE with OUTPUT to atomically increment and return the new value.
        // This is safe under concurrent inserts — no race condition possible.
        // UPDLOCK + SERIALIZABLE hint prevents phantom reads during the upsert.
        var sql = """
            MERGE PatientCounters WITH (HOLDLOCK) AS target
            USING (SELECT {0} AS ClinicId) AS source ON target.ClinicId = source.ClinicId
            WHEN MATCHED THEN
                UPDATE SET LastValue = target.LastValue + 1
            WHEN NOT MATCHED THEN
                INSERT (ClinicId, LastValue) VALUES ({0}, 1)
            OUTPUT inserted.LastValue;
            """;

        var result = await _db.Database
            .SqlQueryRaw<int>(sql, clinicId)
            .FirstAsync(ct);

        // Zero-pad to 7 digits: 1 → "0000001", 9999999 → "9999999"
        // Supports up to 9,999,999 patients per clinic
        return result.ToString("D7");
    }
}
