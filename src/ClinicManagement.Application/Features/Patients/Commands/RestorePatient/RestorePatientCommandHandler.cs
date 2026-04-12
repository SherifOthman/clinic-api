using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class RestorePatientCommandHandler : IRequestHandler<RestorePatientCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public RestorePatientCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(RestorePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _uow.Patients.GetDeletedByIdAsync(request.Id, cancellationToken);

        if (patient is null)
            return Result.Failure(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");

        if (!patient.IsDeleted)
            return Result.Failure(ErrorCodes.PATIENT_NOT_DELETED, "Patient is not deleted");

        patient.Restore();
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
