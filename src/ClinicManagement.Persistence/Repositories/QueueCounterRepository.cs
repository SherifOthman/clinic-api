using ClinicManagement.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class QueueCounterRepository : IQueueCounterRepository
{
    private readonly ApplicationDbContext _db;

    public QueueCounterRepository(ApplicationDbContext db) => _db = db;

    public async Task<int> NextAsync(Guid doctorInfoId, DateOnly date, CancellationToken ct = default)
    {
        // UPDATE existing row and return the new value.
        // UPDLOCK prevents two concurrent readers from both seeing "no row"
        // and racing to insert. HOLDLOCK holds the lock until the transaction ends.
        // If no row exists yet (@@ROWCOUNT = 0), insert the first one and return 1.
        var dateStr = date.ToString("yyyy-MM-dd");

        var sql = $"""
            UPDATE QueueCounters WITH (UPDLOCK, HOLDLOCK)
            SET LastValue = LastValue + 1
            OUTPUT inserted.LastValue
            WHERE DoctorInfoId = '{doctorInfoId}' AND Date = CAST('{dateStr}' AS date);

            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO QueueCounters (DoctorInfoId, Date, LastValue)
                VALUES ('{doctorInfoId}', CAST('{dateStr}' AS date), 1);
                SELECT 1;
            END
            """;

        return await _db.Database
            .SqlQueryRaw<int>(sql)
            .AsAsyncEnumerable()
            .FirstAsync(ct);
    }
}
