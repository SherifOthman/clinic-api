using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
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
        IEnumerable<Appointment> appointments;

        // Apply filters based on query parameters
        if (request.Date.HasValue && request.DoctorId.HasValue)
        {
            appointments = await _unitOfWork.Appointments.GetByDoctorAndDateAsync(request.DoctorId.Value, request.Date.Value, cancellationToken);
        }
        else if (request.Date.HasValue)
        {
            appointments = await _unitOfWork.Appointments.GetByDateAsync(request.Date.Value, cancellationToken);
        }
        else if (request.PatientId.HasValue)
        {
            appointments = await _unitOfWork.Appointments.GetByPatientAsync(request.PatientId.Value, cancellationToken);
        }
        else if (request.AppointmentTypeId.HasValue)
        {
            appointments = await _unitOfWork.Appointments.GetByTypeAsync(request.AppointmentTypeId.Value, cancellationToken);
        }
        else if (request.Status.HasValue)
        {
            appointments = await _unitOfWork.Appointments.GetByStatusAsync(request.Status.Value, cancellationToken);
        }
        else
        {
            // Get all appointments (consider adding pagination in real scenarios)
            appointments = await _unitOfWork.Appointments.GetAllAsync(cancellationToken);
        }

        // Apply additional filters if needed
        if (request.AppointmentTypeId.HasValue && !request.Date.HasValue)
        {
            appointments = appointments.Where(a => a.AppointmentTypeId == request.AppointmentTypeId.Value);
        }

        if (request.Status.HasValue && !request.Date.HasValue)
        {
            appointments = appointments.Where(a => a.Status == request.Status.Value);
        }

        var dtos = appointments.Adapt<IEnumerable<AppointmentDto>>();
        return Result<IEnumerable<AppointmentDto>>.Ok(dtos);
    }
}
