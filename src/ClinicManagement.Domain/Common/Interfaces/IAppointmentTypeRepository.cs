using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IAppointmentTypeRepository : IRepository<AppointmentType>
{
    Task<IEnumerable<AppointmentType>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<AppointmentType?> GetByNameAsync(string nameEn, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppointmentType>> GetOrderedAsync(CancellationToken cancellationToken = default);
}
