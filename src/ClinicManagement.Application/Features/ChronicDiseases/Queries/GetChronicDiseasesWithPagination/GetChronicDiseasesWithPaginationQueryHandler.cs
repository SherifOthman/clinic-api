using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseasesWithPagination;

public class GetChronicDiseasesWithPaginationQueryHandler : IRequestHandler<GetChronicDiseasesWithPaginationQuery, Result<PagedResult<ChronicDiseaseDto>>>
{
    private readonly IRepository<ChronicDisease> _repository;
    private readonly ILogger<GetChronicDiseasesWithPaginationQueryHandler> _logger;

    public GetChronicDiseasesWithPaginationQueryHandler(
        IRepository<ChronicDisease> repository,
        ILogger<GetChronicDiseasesWithPaginationQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ChronicDiseaseDto>>> Handle(GetChronicDiseasesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var paginationRequest = new PaginationRequest
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var pagedResult = await _repository.GetPagedAsync(paginationRequest, cancellationToken);
        var dtos = pagedResult.Items.Adapt<IEnumerable<ChronicDiseaseDto>>();
        
        var result = new PagedResult<ChronicDiseaseDto>(
            dtos,
            pagedResult.TotalCount,
            pagedResult.PageNumber,
            pagedResult.PageSize);

        _logger.LogInformation("Retrieved {Count} chronic diseases (page {PageNumber}/{TotalPages})", 
            result.Items.Count(), result.PageNumber, result.TotalPages);

        return Result<PagedResult<ChronicDiseaseDto>>.Ok(result);
    }
}
