using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Patients.Commands.DeletePatient;

public class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeletePatientCommandHandler> _logger;

    public DeletePatientCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<DeletePatientCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.TryGetUserId(out var userId))
        {
            _logger.LogWarning("Unauthenticated user attempted to delete patient {PatientId}", request.Id);
            return Result.Fail(MessageCodes.Authentication.USER_NOT_AUTHENTICATED);
        }

        if (!_currentUserService.TryGetClinicId(out var userClinicId))
        {
            _logger.LogWarning("User {UserId} without clinic access attempted to delete patient {PatientId}", userId, request.Id);
            return Result.Fail(MessageCodes.Authorization.USER_NO_CLINIC_ACCESS);
        }

        var patient = await _unitOfWork.Patients.GetByIdAsync(request.Id, cancellationToken);
        if (patient == null)
        {
            _logger.LogWarning("Attempt to delete non-existent patient {PatientId} by user {UserId}", request.Id, userId);
            return Result.Fail(MessageCodes.Business.PATIENT_NOT_FOUND);
        }

        if (patient.ClinicId != userClinicId)
        {
            _logger.LogWarning("User {UserId} from clinic {UserClinicId} attempted to delete patient {PatientId} from clinic {PatientClinicId}", 
                userId, userClinicId, request.Id, patient.ClinicId);
            return Result.Fail(MessageCodes.Business.PATIENT_NOT_FOUND);
        }

        patient.SoftDelete(userId);

        await _unitOfWork.Patients.UpdateAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Patient {PatientId} soft deleted by user {UserId}", request.Id, userId);

        return Result.Ok();
    }
}
