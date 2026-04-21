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

        // Zero-pad to 4 digits: 1 → "0001", 9999 → "9999"
        // Stored as string so StartsWith search works (e.g. "12" finds "0012", "0120", "0121")
        return result.ToString("D4");
    }
}
