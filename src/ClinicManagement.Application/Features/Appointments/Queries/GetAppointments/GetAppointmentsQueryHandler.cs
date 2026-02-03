using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, Result<IEnumerable<AppointmentDto>>>
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAppointmentsQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }

    public async Task<Result<IEnumerable<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Domain.Entities.Appointment> appointments;

        // Apply filters based on query parameters
        if (request.Date.HasValue && request.DoctorId.HasValue)
        {
            appointments = await _appointmentRepository.GetByDoctorAndDateAsync(request.DoctorId.Value, request.Date.Value, cancellationToken);
        }
        else if (request.Date.HasValue)
        {
            appointments = await _appointmentRepository.GetByDateAsync(request.Date.Value, cancellationToken);
        }
        else if (request.PatientId.HasValue)
        {
            appointments = await _appointmentRepository.GetByPatientAsync(request.PatientId.Value, cancellationToken);
        }
        else if (request.AppointmentTypeId.HasValue)
        {
            appointments = await _appointmentRepository.GetByTypeAsync(request.AppointmentTypeId.Value, cancellationToken);
        }
        else if (request.Status.HasValue)
        {
            appointments = await _appointmentRepository.GetByStatusAsync(request.Status.Value, cancellationToken);
        }
        else
        {
            // Get all appointments (consider adding pagination in real scenarios)
            appointments = await _appointmentRepository.GetAllAsync(cancellationToken);
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