using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<int> GetCountForClinicBranchByYearAsync(Guid clinicBranchId, int year, CancellationToken cancellationToken = default);
    Task<PagedResult<Appointment>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetFilteredAsync(Guid clinicBranchId, DateTime? date, Guid? doctorId, CancellationToken cancellationToken = default);
    Task<int> GetNextQueueNumberAsync(Guid clinicBranchId, DateTime date, CancellationToken cancellationToken = default);
    Task<bool> HasQueueConflictAsync(Guid clinicBranchId, DateTime date, int queueNumber, CancellationToken cancellationToken = default);
}
