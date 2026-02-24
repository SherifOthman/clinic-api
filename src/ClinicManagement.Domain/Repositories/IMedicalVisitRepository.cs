using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IMedicalVisitRepository : IRepository<MedicalVisit>
{
    Task<IEnumerable<MedicalVisit>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MedicalVisit>> GetByDoctorIdAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<MedicalVisit?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}
