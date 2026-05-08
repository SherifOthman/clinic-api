using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

public class GetAppointmentsHandler : IRequestHandler<GetAppointmentsQuery, Result<List<AppointmentDto>>>
{
    private readonly IAppointmentRepository _appointments;
    private readonly ICurrentUserService    _currentUser;

    public GetAppointmentsHandler(IAppointmentRepository appointments, ICurrentUserService currentUser)
    {
        _appointments = appointments;
        _currentUser  = currentUser;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken ct)
    {
        List<AppointmentDto> list;

        if (request.DoctorInfoIds is { Count: > 0 })
            list = await _appointments.GetProjectedByDoctorsAndDateAsync(request.DoctorInfoIds, request.Date, ct);
        else if (request.BranchId.HasValue)
            list = await _appointments.GetProjectedByBranchAndDateAsync(request.BranchId.Value, request.Date, ct);
        else
            list = [];

        return Result.Success(list);
    }
}
