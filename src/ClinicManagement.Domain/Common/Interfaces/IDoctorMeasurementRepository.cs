using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IDoctorMeasurementRepository : IRepository<DoctorMeasurement>
{
    Task<List<DoctorMeasurement>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<List<DoctorMeasurement>> GetEnabledByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task BulkInsertAsync(List<DoctorMeasurement> measurements, CancellationToken cancellationToken = default);
}