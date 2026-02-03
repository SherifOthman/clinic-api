using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IVisitMeasurementRepository : IRepository<VisitMeasurement>
{
    Task<List<VisitMeasurement>> GetByVisitIdAsync(Guid visitId, CancellationToken cancellationToken = default);
    Task<List<VisitMeasurement>> GetByPatientIdAsync(Guid clinicPatientId, CancellationToken cancellationToken = default);
}