using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

public class GetAppointmentsHandler : IRequestHandler<GetAppointmentsQuery, Result<List<AppointmentDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetAppointmentsHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken ct)
    {
        // Use projected queries — mapping happens in SQL, no entity materialisation needed
        List<AppointmentDto> list;

        if (request.DoctorInfoIds is { Count: > 0 })
            list = await _uow.Appointments.GetProjectedByDoctorsAndDateAsync(request.DoctorInfoIds, request.Date, ct);
        else if (request.BranchId.HasValue)
            list = await _uow.Appointments.GetProjectedByBranchAndDateAsync(request.BranchId.Value, request.Date, ct);
        else
            list = [];

        return Result.Success(list);
    }
}
