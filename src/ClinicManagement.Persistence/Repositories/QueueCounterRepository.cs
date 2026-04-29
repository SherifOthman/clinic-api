using ClinicManagement.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class QueueCounterRepository : IQueueCounterRepository
{
    private readonly ApplicationDbContext _db;

    public QueueCounterRepository(ApplicationDbContext db) => _db = db;

    public Task<int> NextAsync(Guid doctorInfoId, DateOnly date, CancellationToken ct = default)
    {
        // SQL Server date literal: 'YYYY-MM-DD'
        var dateStr = date.ToString("yyyy-MM-dd");

        var sql = $"""
            MERGE QueueCounters WITH (HOLDLOCK) AS target
            USING (SELECT '{doctorInfoId}' AS DoctorInfoId, CAST('{dateStr}' AS date) AS Date) AS source
                ON target.DoctorInfoId = source.DoctorInfoId AND target.Date = source.Date
            WHEN MATCHED THEN
                UPDATE SET LastValue = target.LastValue + 1
            WHEN NOT MATCHED THEN
                INSERT (DoctorInfoId, Date, LastValue) VALUES ('{doctorInfoId}', CAST('{dateStr}' AS date), 1)
            OUTPUT inserted.LastValue;
            """;

        // MERGE OUTPUT is non-composable — pull to client before First()
        return Task.FromResult(
            _db.Database
               .SqlQueryRaw<int>(sql)
               .AsEnumerable()
               .First());
    }
}
