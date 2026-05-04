using ClinicManagement.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class PatientCounterRepository : IPatientCounterRepository
{
    private readonly ApplicationDbContext _db;

    public PatientCounterRepository(ApplicationDbContext db) => _db = db;

    public async Task<string> NextCodeAsync(Guid clinicId, CancellationToken ct = default)
    {
        // UPDATE existing row and return the new value.
        // UPDLOCK prevents two concurrent readers from both seeing "no row"
        // and racing to insert. HOLDLOCK holds the lock until the transaction ends.
        // If no row exists yet (@@ROWCOUNT = 0), insert the first one and return 1.
        var sql = """
            UPDATE PatientCounters WITH (UPDLOCK, HOLDLOCK)
            SET LastValue = LastValue + 1
            OUTPUT inserted.LastValue
            WHERE ClinicId = {0};

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO PatientCounters (ClinicId, LastValue) VALUES ({0}, 1);
                SELECT 1;
            END
            """;

        var result = await _db.Database
            .SqlQueryRaw<int>(sql, clinicId)
            .AsAsyncEnumerable()
            .FirstAsync(ct);

        // Zero-pad to 4 digits: 1 → "0001", 9999 → "9999"
        // Stored as string so StartsWith search works (e.g. "12" finds "0012", "0120", "0121")
        return result.ToString("D4");
    }
}
