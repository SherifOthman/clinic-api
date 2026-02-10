using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetAppointmentsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        // Get all appointments and filter in memory
        // Note: In production, you'd want to add clinic/branch filtering at the database level
        var allAppointments = await _unitOfWork.Appointments.GetAllAsync(cancellationToken);
        
        var filtered = allAppointments.AsEnumerable();
        
        if (request.Date.HasValue)
        {
            filtered = filtered.Where(a => a.AppointmentDate.Date == request.Date.Value.Date);
        }
        
        if (request.DoctorId.HasValue)
        {
            filtered = filtered.Where(a => a.DoctorId == request.DoctorId.Value);
        }
        
        if (request.PatientId.HasValue)
        {
            filtered = filtered.Where(a => a.PatientId == request.PatientId.Value);
        }
        
        if (request.AppointmentTypeId.HasValue)
        {
            filtered = filtered.Where(a => a.AppointmentTypeId == request.AppointmentTypeId.Value);
        }
        
        if (request.Status.HasValue)
        {
            filtered = filtered.Where(a => a.Status == request.Status.Value);
        }

        var appointmentDtos = filtered.Adapt<IEnumerable<AppointmentDto>>();
        return Result<IEnumerable<AppointmentDto>>.Ok(appointmentDtos);
    }
}
