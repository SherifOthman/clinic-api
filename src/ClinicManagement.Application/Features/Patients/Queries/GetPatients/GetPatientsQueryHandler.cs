using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatients;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<List<PatientDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetPatientsQueryHandler> _logger;

    public GetPatientsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<GetPatientsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<List<PatientDto>>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasClinicAccess())
        {
            _logger.LogWarning("Unauthorized access attempt to patients by user {UserId}", _currentUserService.UserId);
            return Result<List<PatientDto>>.Fail("Access denied. User must be authenticated and associated with a clinic.");
        }
        
        var patients = await _unitOfWork.Patients.GetAllAsync(cancellationToken);
        var patientDtos = patients.Adapt<List<PatientDto>>();
        
        _logger.LogInformation("Retrieved {Count} patients for clinic {ClinicId}", patientDtos.Count, _currentUserService.ClinicId);
        
        return Result<List<PatientDto>>.Ok(patientDtos);
    }
}
