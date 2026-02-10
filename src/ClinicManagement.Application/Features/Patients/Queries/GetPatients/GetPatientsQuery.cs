using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatients;

public record GetPatientsQuery(
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = null,
    string SortDirection = "desc",
    Gender? Gender = null,
    DateTime? DateOfBirthFrom = null,
    DateTime? DateOfBirthTo = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    int? MinAge = null,
    int? MaxAge = null
) : IRequest<Result<PagedResult<PatientDto>>>;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PagedResult<PatientDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetPatientsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<PatientDto>>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.ClinicId!.Value;

        // Build filters dictionary
        var filters = new Dictionary<string, object>();
        
        if (request.Gender.HasValue)
            filters["gender"] = request.Gender.Value;
        
        if (request.DateOfBirthFrom.HasValue)
            filters["dateOfBirthFrom"] = request.DateOfBirthFrom.Value;
        
        if (request.DateOfBirthTo.HasValue)
            filters["dateOfBirthTo"] = request.DateOfBirthTo.Value;
        
        if (request.CreatedFrom.HasValue)
            filters["createdFrom"] = request.CreatedFrom.Value;
        
        if (request.CreatedTo.HasValue)
            filters["createdTo"] = request.CreatedTo.Value;
        
        if (request.MinAge.HasValue)
            filters["minAge"] = request.MinAge.Value;
        
        if (request.MaxAge.HasValue)
            filters["maxAge"] = request.MaxAge.Value;

        var paginationRequest = new SearchablePaginationRequest
        {
            SearchTerm = request.SearchTerm,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Filters = filters
        };

        var pagedPatients = await _unitOfWork.Patients.GetPagedForClinicAsync(clinicId, paginationRequest, cancellationToken);

        var dtos = pagedPatients.Items.Adapt<List<PatientDto>>();
        var pagedResult = new PagedResult<PatientDto>(
            dtos,
            pagedPatients.TotalCount,
            pagedPatients.PageNumber,
            pagedPatients.PageSize);

        return Result<PagedResult<PatientDto>>.Ok(pagedResult);
    }
}
