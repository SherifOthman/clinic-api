using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
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
    private readonly IApplicationDbContext _context;

    public GetChronicDiseasesPaginatedQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<ChronicDiseaseDto>>> Handle(GetChronicDiseasesPaginatedQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ChronicDiseases.AsNoTracking();
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var chronicDiseases = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        var dtos = chronicDiseases.Adapt<List<ChronicDiseaseDto>>();
        var result = new PagedResult<ChronicDiseaseDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PagedResult<ChronicDiseaseDto>>.Ok(result);
    }
}
