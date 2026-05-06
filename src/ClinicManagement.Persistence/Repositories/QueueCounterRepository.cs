using ClinicManagement.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class QueueCounterRepository : IQueueCounterRepository
{
    private readonly ApplicationDbContext _db;

    public QueueCounterRepository(ApplicationDbContext db) => _db = db;

    public async Task<int> NextAsync(Guid doctorInfoId, DateOnly date, CancellationToken ct = default)
    {
        // MERGE is a single atomic statement — the read, match, and write happen
        // as one unit, so no explicit transaction or locking hints are needed.
        // HOLDLOCK prevents phantom inserts between the WHEN NOT MATCHED check
        // and the INSERT on the first call for a new doctor/date combination.
        //
        // On first call for a doctor/date, we seed LastValue from the MAX existing
        // QueueNumber in the Appointments table. This handles the case where
        // appointments were seeded directly (bypassing the counter) — without this,
        // the counter would start at 1 and collide with existing rows.
        var dateStr = date.ToString("yyyy-MM-dd");

        var sql = $"""
            MERGE QueueCounters WITH (HOLDLOCK) AS target
            USING (SELECT '{doctorInfoId}' AS DoctorInfoId, CAST('{dateStr}' AS date) AS Date) AS source
                ON target.DoctorInfoId = source.DoctorInfoId AND target.Date = source.Date
            WHEN MATCHED THEN
                UPDATE SET LastValue = target.LastValue + 1
            WHEN NOT MATCHED THEN
                INSERT (DoctorInfoId, Date, LastValue)
                VALUES (
                    '{doctorInfoId}',
                    CAST('{dateStr}' AS date),
                    ISNULL((SELECT MAX(QueueNumber) FROM Appointments
                             WHERE DoctorInfoId = '{doctorInfoId}'
                               AND Date = CAST('{dateStr}' AS date)
                               AND IsDeleted = 0), 0) + 1
                )
            OUTPUT inserted.LastValue;
            """;

        return await _db.Database
            .SqlQueryRaw<int>(sql)
            .AsAsyncEnumerable()
            .FirstAsync(ct);
    }
}
