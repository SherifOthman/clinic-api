using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery(
    DateTime? Date = null,
    Guid? DoctorId = null,
    Guid? PatientId = null,
    Guid? AppointmentTypeId = null,
    AppointmentStatus? Status = null
) : IRequest<Result<IEnumerable<AppointmentDto>>>;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, Result<IEnumerable<AppointmentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments.AsNoTracking().AsQueryable();

        // Apply filters
        if (request.Date.HasValue)
        {
            var date = request.Date.Value.Date;
            query = query.Where(a => a.AppointmentDate.Date == date);
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

        var appointments = await query
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.QueueNumber)
            .ToListAsync(cancellationToken);

        var appointmentDtos = appointments.Adapt<IEnumerable<AppointmentDto>>();
        return Result<IEnumerable<AppointmentDto>>.Ok(appointmentDtos);
    }
}
