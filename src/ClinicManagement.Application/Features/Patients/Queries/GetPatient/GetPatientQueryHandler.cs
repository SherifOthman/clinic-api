using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatient;

public class GetPatientQueryHandler : IRequestHandler<GetPatientQuery, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetPatientQueryHandler> _logger;

    public GetPatientQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<GetPatientQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<PatientDto>> Handle(GetPatientQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasClinicAccess())
        {
            _logger.LogWarning("Unauthorized access attempt to patient {PatientId} by user {UserId}", request.Id, _currentUserService.UserId);
            return Result<PatientDto>.Fail(MessageCodes.Authorization.USER_NO_CLINIC_ACCESS);
        }

        var patient = await _unitOfWork.Patients.GetByIdAsync(request.Id, cancellationToken);
        if (patient == null)
        {
            _logger.LogWarning("Patient with ID {Id} not found or access denied for clinic {ClinicId}", request.Id, _currentUserService.ClinicId);
            return Result<PatientDto>.Fail(MessageCodes.Business.PATIENT_NOT_FOUND);
        }

        var patientDto = patient.Adapt<PatientDto>();
        
        return Result<PatientDto>.Ok(patientDto);
    }
}
