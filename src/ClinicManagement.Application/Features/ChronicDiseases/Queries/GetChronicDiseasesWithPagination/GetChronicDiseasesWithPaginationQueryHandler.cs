using ClinicManagement.Application.Common.Extensions;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseasesWithPagination;

public class GetChronicDiseasesWithPaginationQueryHandler : IRequestHandler<GetChronicDiseasesWithPaginationQuery, Result<PagedResult<ChronicDiseaseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetChronicDiseasesWithPaginationQueryHandler> _logger;

    public GetChronicDiseasesWithPaginationQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetChronicDiseasesWithPaginationQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ChronicDiseaseDto>>> Handle(GetChronicDiseasesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var paginationRequest = new PaginationRequest
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var query = (await _unitOfWork.ChronicDiseases.GetAllAsync(cancellationToken)).AsQueryable();
        
        var pagedResult = await query.ToPaginatedResultAsync(paginationRequest, cancellationToken);
        var dtos = pagedResult.Items.Adapt<IEnumerable<ChronicDiseaseDto>>();
        
        var result = new PagedResult<ChronicDiseaseDto>(
            dtos.ToList(),
            pagedResult.TotalCount,
            pagedResult.PageNumber,
            pagedResult.PageSize);

        _logger.LogInformation("Retrieved {Count} chronic diseases (page {PageNumber}/{TotalPages})", 
            result.Items.Count, result.PageNumber, result.TotalPages);

        return Result<PagedResult<ChronicDiseaseDto>>.Ok(result);
    }
}
