using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseasesPaginated;

public record GetChronicDiseasesPaginatedQuery(
    int PageNumber,
    int PageSize
) : IRequest<Result<PagedResult<ChronicDiseaseDto>>>;

public class GetChronicDiseasesPaginatedQueryHandler : IRequestHandler<GetChronicDiseasesPaginatedQuery, Result<PagedResult<ChronicDiseaseDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetChronicDiseasesPaginatedQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<ChronicDiseaseDto>>> Handle(GetChronicDiseasesPaginatedQuery request, CancellationToken cancellationToken)
    {
        var paginationRequest = new PaginationRequest 
        { 
            PageNumber = request.PageNumber, 
            PageSize = request.PageSize 
        };
        var result = await _unitOfWork.ChronicDiseases.GetPagedAsync(paginationRequest, cancellationToken);

        var dtos = result.Items.Adapt<List<ChronicDiseaseDto>>();
        var pagedResult = new PagedResult<ChronicDiseaseDto>(dtos, result.TotalCount, result.PageNumber, result.PageSize);

        return Result<PagedResult<ChronicDiseaseDto>>.Ok(pagedResult);
    }
}
