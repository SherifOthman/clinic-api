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
            .Where(a => a.AppointmentDate.Date == date.Date)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.AppointmentType)
            .Include(a => a.ClinicBranch)
            .OrderBy(a => a.QueueNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.AppointmentType)
            .Include(a => a.ClinicBranch)
            .OrderBy(a => a.QueueNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByPatientAsync(Guid PatientId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.PatientId == PatientId)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.AppointmentType)
            .Include(a => a.ClinicBranch)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByTypeAsync(Guid appointmentTypeId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.AppointmentTypeId == appointmentTypeId)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.AppointmentType)
            .Include(a => a.ClinicBranch)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.Status == status)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.AppointmentType)
            .Include(a => a.ClinicBranch)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Appointment?> GetByQueueNumberAsync(DateTime date, Guid doctorId, short queueNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.AppointmentDate.Date == date.Date && a.DoctorId == doctorId && a.QueueNumber == queueNumber)
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.AppointmentType)
            .Include(a => a.ClinicBranch)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<short> GetNextQueueNumberAsync(DateTime date, Guid doctorId, CancellationToken cancellationToken = default)
    {
        var maxQueueNumber = await _context.Appointments
            .Where(a => a.AppointmentDate.Date == date.Date && a.DoctorId == doctorId)
            .MaxAsync(a => (short?)a.QueueNumber, cancellationToken);

        return (short)(maxQueueNumber.GetValueOrDefault() + 1);
    }

    public async Task<bool> HasConflictingAppointmentAsync(Guid doctorId, DateTime appointmentDate, short queueNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId && 
                       a.AppointmentDate.Date == appointmentDate.Date && 
                       a.QueueNumber == queueNumber)
            .AnyAsync(cancellationToken);
    }

    public async Task<int> GetAppointmentCountForDateAsync(Guid doctorId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .CountAsync(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date, cancellationToken);
    }
}