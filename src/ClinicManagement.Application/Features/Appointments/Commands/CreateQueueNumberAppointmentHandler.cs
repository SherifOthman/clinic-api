using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

/// <summary>
/// Placeholder handler for queue-based appointment creation.
/// Not yet implemented — returns a clear error until the feature is built.
/// Needs: IAppointmentRepository + IQueueNumberRepository added to IUnitOfWork.
/// </summary>
public class CreateQueueNumberAppointmentHandler
    : IRequestHandler<CreateQueueNumberAppointmentCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(
        CreateQueueNumberAppointmentCommand request,
        CancellationToken cancellationToken)
        => Task.FromResult(
            Result.Failure<Guid>(ErrorCodes.OPERATION_FAILED, "Appointment creation not yet implemented"));
}
