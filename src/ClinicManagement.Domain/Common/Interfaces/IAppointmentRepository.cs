using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByPatientAsync(Guid PatientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByTypeAsync(Guid appointmentTypeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default);
    Task<Appointment?> GetByQueueNumberAsync(DateTime date, Guid doctorId, short queueNumber, CancellationToken cancellationToken = default);
    Task<short> GetNextQueueNumberAsync(DateTime date, Guid doctorId, CancellationToken cancellationToken = default);
    Task<bool> HasConflictingAppointmentAsync(Guid doctorId, DateTime appointmentDate, short queueNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Appointment>> GetWithFiltersAsync(
        DateTime? date = null,
        Guid? doctorId = null,
        Guid? patientId = null,
        Guid? appointmentTypeId = null,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default);
}
