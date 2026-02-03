using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class AppointmentRepository : BaseRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.ClinicPatient)
            .Include(a => a.Doctor)
            .Include(a => a.AppointmentType)
            .Where(a => a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.QueueNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.ClinicPatient)
            .Include(a => a.Doctor)
            .Include(a => a.AppointmentType)
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.QueueNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByPatientAsync(Guid clinicPatientId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.ClinicPatient)
            .Include(a => a.Doctor)
            .Include(a => a.AppointmentType)
            .Where(a => a.ClinicPatientId == clinicPatientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByTypeAsync(Guid appointmentTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.ClinicPatient)
            .Include(a => a.Doctor)
            .Include(a => a.AppointmentType)
            .Where(a => a.AppointmentTypeId == appointmentTypeId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.ClinicPatient)
            .Include(a => a.Doctor)
            .Include(a => a.AppointmentType)
            .Where(a => a.Status == status)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.QueueNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetByQueueNumberAsync(DateTime date, Guid doctorId, short queueNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Include(a => a.ClinicPatient)
            .Include(a => a.Doctor)
            .Include(a => a.AppointmentType)
            .FirstOrDefaultAsync(a => a.AppointmentDate.Date == date.Date && 
                                    a.DoctorId == doctorId && 
                                    a.QueueNumber == queueNumber, cancellationToken);
    }

    public async Task<short> GetNextQueueNumberAsync(DateTime date, Guid doctorId, CancellationToken cancellationToken = default)
    {
        var maxQueueNumber = await _context.Appointments
            .Where(a => a.AppointmentDate.Date == date.Date && a.DoctorId == doctorId)
            .MaxAsync(a => (short?)a.QueueNumber, cancellationToken);

        return (short)(maxQueueNumber.GetValueOrDefault() + 1);
    }
}
