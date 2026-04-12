using ClinicManagement.Application.Features.Staff.QueryModels;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IWorkingDaysRepository
{
    Task<List<WorkingDayRow>> GetByDoctorProfileIdAsync(Guid doctorProfileId, CancellationToken ct = default);
    Task<List<DoctorWorkingDay>> GetEntitiesByDoctorProfileIdAsync(Guid doctorProfileId, CancellationToken ct = default);

    void Add(DoctorWorkingDay day);
    void RemoveRange(IEnumerable<DoctorWorkingDay> days);
}
