using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByPatientAsync(Guid clinicPatientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByTypeAsync(Guid appointmentTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByQueueNumberAsync(DateTime date, Guid doctorId, short queueNumber, CancellationToken cancellationToken = default);
    Task<short> GetNextQueueNumberAsync(DateTime date, Guid doctorId, CancellationToken cancellationToken = default);
}