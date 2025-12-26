using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.PatientId == patientId)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDoctorIdAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.DoctorId == doctorId)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByBranchIdAsync(int branchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.BranchId == branchId)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByStatusAsync(AppointmentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.Status == status)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate <= endDate)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetUpcomingAppointmentsAsync(int doctorId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(a => a.DoctorId == doctorId && 
                       a.AppointmentDate >= now && 
                       a.Status == AppointmentStatus.Scheduled)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Appointment>> GetOverdueAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var overdueTime = DateTime.UtcNow.AddMinutes(-15); // 15 minutes grace period
        return await _dbSet
            .Where(a => a.Status == AppointmentStatus.Scheduled && a.AppointmentDate < overdueTime)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(cancellationToken);
    }
}

