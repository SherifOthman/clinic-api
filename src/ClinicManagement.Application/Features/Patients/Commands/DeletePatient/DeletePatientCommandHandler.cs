using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public DeletePatientCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _uow.Patients.GetByIdAsync(request.Id, cancellationToken);

        if (patient is null)
            return Result.Failure(ErrorCodes.PATIENT_NOT_FOUND, "Patient not found");

        patient.SoftDelete();
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
