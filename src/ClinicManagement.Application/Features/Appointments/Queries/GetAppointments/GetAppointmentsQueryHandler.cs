using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, Result<IEnumerable<AppointmentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
            .Include(a => a.AppointmentType)
            .Include(a => a.ClinicBranch)
            .AsQueryable();

        // Apply filters
        if (request.Date.HasValue)
        {
            query = query.Where(a => a.AppointmentDate.Date == request.Date.Value.Date);
        }

        if (request.DoctorId.HasValue)
        {
            query = query.Where(a => a.DoctorId == request.DoctorId.Value);
        }

        if (request.PatientId.HasValue)
        {
            query = query.Where(a => a.PatientId == request.PatientId.Value);
        }

        if (request.AppointmentTypeId.HasValue)
        {
            query = query.Where(a => a.AppointmentTypeId == request.AppointmentTypeId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        // Order by appointment date and queue number
        query = query.OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.QueueNumber);

        // Project to DTO to avoid loading full entities
        var appointments = await query
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                AppointmentNumber = a.AppointmentNumber,
                AppointmentDate = a.AppointmentDate,
                QueueNumber = a.QueueNumber,
                Status = a.Status,
                FinalPrice = a.FinalPrice,
                DiscountAmount = a.DiscountAmount,
                PaidAmount = a.PaidAmount,
                RemainingAmount = a.RemainingAmount,
                ClinicBranchId = a.ClinicBranchId,
                PatientId = a.PatientId,
                DoctorId = a.DoctorId,
                AppointmentTypeId = a.AppointmentTypeId,
                // Navigation properties
                PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                DoctorName = $"{a.Doctor.User.FirstName} {a.Doctor.User.LastName}",
                AppointmentTypeName = a.AppointmentType.NameEn, // Using English name as default
                ClinicBranchName = a.ClinicBranch.Name
            })
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<AppointmentDto>>.Ok(appointments);
    }
}