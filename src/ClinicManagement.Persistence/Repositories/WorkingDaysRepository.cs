using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class WorkingDaysRepository : Repository<DoctorWorkingDay>, IWorkingDaysRepository
{
    public WorkingDaysRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<WorkingDayRow>> GetByDoctorProfileIdAsync(
        Guid doctorProfileId, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .Where(d => d.DoctorId == doctorProfileId)
            .OrderBy(d => d.ClinicBranchId).ThenBy(d => d.Day)
            .Select(d => new WorkingDayRow(
                (int)d.Day,
                d.StartTime.ToString("HH:mm"),
                d.EndTime.ToString("HH:mm"),
                d.IsAvailable,
                d.ClinicBranchId,
                d.MaxAppointmentsPerDay))
            .ToListAsync(ct);

    public async Task<List<DoctorWorkingDay>> GetEntitiesByDoctorProfileIdAsync(
        Guid doctorProfileId, CancellationToken ct = default)
        => await DbSet.Where(d => d.DoctorId == doctorProfileId).ToListAsync(ct);

    public void Add(DoctorWorkingDay day)                        => DbSet.Add(day);
    public void RemoveRange(IEnumerable<DoctorWorkingDay> days)  => DbSet.RemoveRange(days);
}
