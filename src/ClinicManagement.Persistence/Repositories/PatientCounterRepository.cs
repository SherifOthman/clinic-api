using ClinicManagement.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class PatientCounterRepository : IPatientCounterRepository
{
    private readonly ApplicationDbContext _db;

    public PatientCounterRepository(ApplicationDbContext db) => _db = db;

    public async Task<string> NextCodeAsync(Guid clinicId, CancellationToken ct = default)
    {
        // Parameterized with {0} — avoids string interpolation into SQL.
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
            .AsAsyncEnumerable()
            .FirstAsync(ct);

        return result.ToString("D4");
    }
}
