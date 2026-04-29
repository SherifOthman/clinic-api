using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

public class GetAppointmentsHandler : IRequestHandler<GetAppointmentsQuery, List<AppointmentDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetAppointmentsHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<List<AppointmentDto>> Handle(GetAppointmentsQuery request, CancellationToken ct)
    {
        List<Domain.Entities.Appointment> appointments;

        if (request.DoctorInfoIds is { Count: > 0 })
            appointments = await _uow.Appointments.GetByDoctorsAndDateAsync(request.DoctorInfoIds, request.Date, ct);
        else if (request.BranchId.HasValue)
            appointments = await _uow.Appointments.GetByBranchAndDateAsync(request.BranchId.Value, request.Date, ct);
        else
            appointments = [];

        return appointments.Select(a => new AppointmentDto(
            a.Id,
            a.DoctorInfoId,
            a.Doctor?.ClinicMember?.Person?.FullName ?? "—",
            a.PatientId,
            a.Patient?.Person?.FullName ?? "—",
            a.Patient?.PatientCode,
            a.QueueNumber,
            a.ScheduledTime?.ToString("HH:mm"),
            a.EndTime?.ToString("HH:mm"),
            a.VisitDurationMinutes,
            a.Type.ToString(),
            a.Status.ToString(),
            a.VisitType?.Name ?? "—",
            a.FinalPrice,
            a.CreatedAt
        )).ToList();
    }
}
