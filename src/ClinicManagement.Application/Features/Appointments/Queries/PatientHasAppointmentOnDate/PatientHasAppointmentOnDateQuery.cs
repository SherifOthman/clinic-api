using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

public record PatientHasAppointmentOnDateQuery(Guid PatientId, DateOnly Date) : IRequest<Result<bool>>;

public class PatientHasAppointmentOnDateHandler
    : IRequestHandler<PatientHasAppointmentOnDateQuery, Result<bool>>
{
    private readonly IAppointmentRepository _appointments;

    public PatientHasAppointmentOnDateHandler(IAppointmentRepository appointments) =>
        _appointments = appointments;

    public async Task<Result<bool>> Handle(PatientHasAppointmentOnDateQuery request, CancellationToken ct)
    {
        var exists = await _appointments.PatientHasAppointmentOnDateAsync(request.PatientId, request.Date, ct);
        return Result.Success(exists);
    }
}
