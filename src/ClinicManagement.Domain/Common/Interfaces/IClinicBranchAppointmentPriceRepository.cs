using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IClinicBranchAppointmentPriceRepository : IRepository<ClinicBranchAppointmentPrice>
{
    Task<IEnumerable<ClinicBranchAppointmentPrice>> GetByClinicBranchAsync(Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClinicBranchAppointmentPrice>> GetActiveByClinicBranchAsync(Guid clinicBranchId, CancellationToken cancellationToken = default);
    Task<ClinicBranchAppointmentPrice?> GetByClinicBranchAndTypeAsync(Guid clinicBranchId, Guid appointmentTypeId, CancellationToken cancellationToken = default);
    Task<decimal?> GetPriceAsync(Guid clinicBranchId, Guid appointmentTypeId, CancellationToken cancellationToken = default);
}
