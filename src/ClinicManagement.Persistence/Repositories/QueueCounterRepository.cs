using ClinicManagement.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class QueueCounterRepository : IQueueCounterRepository
{
    private readonly ApplicationDbContext _db;

    public QueueCounterRepository(ApplicationDbContext db) => _db = db;

    public async Task<int> NextAsync(Guid doctorInfoId, DateOnly date, CancellationToken ct = default)
    {
        var dateStr = date.ToString("yyyy-MM-dd");

        // Parameterized with {0}/{1} — avoids string interpolation into SQL.
        // doctorInfoId is a Guid (not user input) but parameterization is the correct habit.
        var sql = """
            MERGE QueueCounters WITH (HOLDLOCK) AS target
            USING (SELECT {0} AS DoctorInfoId, CAST({1} AS date) AS Date) AS source
                ON target.DoctorInfoId = source.DoctorInfoId AND target.Date = source.Date
            WHEN MATCHED THEN
                UPDATE SET LastValue = target.LastValue + 1
            WHEN NOT MATCHED THEN
                INSERT (DoctorInfoId, Date, LastValue)
                VALUES (
                    {0},
                    CAST({1} AS date),
                    ISNULL((SELECT MAX(QueueNumber) FROM Appointment
                             WHERE DoctorInfoId = {0}
                               AND Date = CAST({1} AS date)
                               AND IsDeleted = 0), 0) + 1
                )
            OUTPUT inserted.LastValue;
            """;

        return await _db.Database
            .SqlQueryRaw<int>(sql, doctorInfoId, dateStr)
            .AsAsyncEnumerable()
            .FirstAsync(ct);
    }
}
