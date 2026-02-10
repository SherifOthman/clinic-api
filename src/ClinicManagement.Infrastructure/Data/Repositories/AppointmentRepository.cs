using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class AppointmentRepository : BaseRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Appointment>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(a => a.ClinicBranchId == clinicBranchId);

        var totalCount = await query.CountAsync(cancellationToken);

        // Default sorting by appointment date descending
        query = query.OrderByDescending(a => a.AppointmentDate);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Appointment>(items, totalCount, pageNumber, pageSize);
    }

    protected override IQueryable<Appointment> ApplySearchAndFilters(IQueryable<Appointment> query, SearchablePaginationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(a => a.AppointmentNumber.Contains(request.SearchTerm));
        }

        // Apply filters from dictionary
        if (request.Filters != null && request.Filters.Count > 0)
        {
            if (request.Filters.TryGetValue("clinicBranchId", out var branchIdObj) && branchIdObj is Guid branchId)
            {
                query = query.Where(a => a.ClinicBranchId == branchId);
            }

            if (request.Filters.TryGetValue("patientId", out var patientIdObj) && patientIdObj is Guid patientId)
            {
                query = query.Where(a => a.PatientId == patientId);
            }

            if (request.Filters.TryGetValue("doctorId", out var doctorIdObj) && doctorIdObj is Guid doctorId)
            {
                query = query.Where(a => a.DoctorId == doctorId);
            }

            if (request.Filters.TryGetValue("status", out var statusObj) && statusObj is int status)
            {
                query = query.Where(a => (int)a.Status == status);
            }

            if (request.Filters.TryGetValue("fromDate", out var fromDateObj) && fromDateObj is DateTime fromDate)
            {
                query = query.Where(a => a.AppointmentDate >= fromDate);
            }

            if (request.Filters.TryGetValue("toDate", out var toDateObj) && toDateObj is DateTime toDate)
            {
                query = query.Where(a => a.AppointmentDate <= toDate);
            }
        }

        return query;
    }

    protected override IQueryable<Appointment> ApplySorting(IQueryable<Appointment> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "appointmentdate" or "date" => request.IsAscending 
                ? query.OrderBy(a => a.AppointmentDate) 
                : query.OrderByDescending(a => a.AppointmentDate),
            "appointmentnumber" or "number" => request.IsAscending 
                ? query.OrderBy(a => a.AppointmentNumber) 
                : query.OrderByDescending(a => a.AppointmentNumber),
            "queuenumber" or "queue" => request.IsAscending 
                ? query.OrderBy(a => a.QueueNumber) 
                : query.OrderByDescending(a => a.QueueNumber),
            "status" => request.IsAscending 
                ? query.OrderBy(a => a.Status) 
                : query.OrderByDescending(a => a.Status),
            "finalprice" or "price" => request.IsAscending 
                ? query.OrderBy(a => a.FinalPrice) 
                : query.OrderByDescending(a => a.FinalPrice),
            "createdat" => request.IsAscending 
                ? query.OrderBy(a => a.CreatedAt) 
                : query.OrderByDescending(a => a.CreatedAt),
            _ => query.OrderByDescending(a => a.AppointmentDate)
        };
    }
}
