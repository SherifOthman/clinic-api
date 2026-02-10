using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<PagedResult<Appointment>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
