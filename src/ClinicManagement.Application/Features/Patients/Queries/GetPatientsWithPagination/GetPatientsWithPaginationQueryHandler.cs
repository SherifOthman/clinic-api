using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientsWithPagination;

public class GetPatientsWithPaginationQueryHandler : IRequestHandler<GetPatientsWithPaginationQuery, Result<PagedResult<PatientDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetPatientsWithPaginationQueryHandler> _logger;

    public GetPatientsWithPaginationQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, ILogger<GetPatientsWithPaginationQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<PatientDto>>> Handle(GetPatientsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.HasClinicAccess())
        {
            _logger.LogWarning("Unauthorized paginated patients access attempt by user {UserId}", _currentUserService.UserId);
            return Result<PagedResult<PatientDto>>.Fail(MessageCodes.Authorization.USER_NO_CLINIC_ACCESS);
        }

        var patientSearchRequest = new PatientSearchRequest(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            request.SortBy,
            request.SortDescending)
        {
            Gender = request.Gender,
            DateOfBirthFrom = request.DateOfBirthFrom,
            DateOfBirthTo = request.DateOfBirthTo,
            CreatedFrom = request.CreatedFrom,
            CreatedTo = request.CreatedTo,
            MinAge = request.MinAge,
            MaxAge = request.MaxAge
        };

        var pagedPatients = await _unitOfWork.Patients.GetPagedAsync(patientSearchRequest, cancellationToken);

        var patientDtos = pagedPatients.Items.Adapt<List<PatientDto>>();

        var result = new PagedResult<PatientDto>(
            patientDtos,
            pagedPatients.TotalCount,
            pagedPatients.PageNumber,
            pagedPatients.PageSize);

        _logger.LogInformation("Retrieved {Count} patients (page {PageNumber}) for clinic {ClinicId}", 
            patientDtos.Count, request.PageNumber, _currentUserService.ClinicId);

        return Result<PagedResult<PatientDto>>.Ok(result);
    }
}
