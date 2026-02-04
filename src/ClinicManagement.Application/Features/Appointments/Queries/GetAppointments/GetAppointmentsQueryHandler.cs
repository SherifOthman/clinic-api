using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, Result<IEnumerable<AppointmentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAppointmentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _unitOfWork.Appointments.GetWithFiltersAsync(
            request.Date,
            request.DoctorId,
            request.PatientId,
            request.AppointmentTypeId,
            request.Status,
            cancellationToken);

        var appointmentDtos = appointments.Adapt<IEnumerable<AppointmentDto>>();
        return Result<IEnumerable<AppointmentDto>>.Ok(appointmentDtos);
    }
}