using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.MedicalSupplies.Queries.GetMedicalSupplies;

public record GetMedicalSuppliesQuery(
    Guid ClinicBranchId,
    int? PageNumber = null,
    int? PageSize = null
) : IRequest<Result<PagedResult<MedicalSupplyDto>>>;

public class GetMedicalSuppliesQueryHandler : IRequestHandler<GetMedicalSuppliesQuery, Result<PagedResult<MedicalSupplyDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMedicalSuppliesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<MedicalSupplyDto>>> Handle(GetMedicalSuppliesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MedicalSupplies
            .AsNoTracking()
            .Where(s => s.ClinicBranchId == request.ClinicBranchId)
            .OrderBy(s => s.Name);

        PagedResult<MedicalSupplyDto> result;
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            // Paginated result
            var totalCount = await query.CountAsync(cancellationToken);
            
            var supplies = await query
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value)
                .ToListAsync(cancellationToken);
            
            var suppliesDto = supplies.Adapt<List<MedicalSupplyDto>>();
            result = new PagedResult<MedicalSupplyDto>(suppliesDto, totalCount, request.PageNumber.Value, request.PageSize.Value);
        }
        else
        {
            // Return all items as a single page
            var allSupplies = await query.ToListAsync(cancellationToken);
            var suppliesDto = allSupplies.Adapt<List<MedicalSupplyDto>>();
            result = new PagedResult<MedicalSupplyDto>(suppliesDto, suppliesDto.Count, 1, suppliesDto.Count);
        }
        
        return Result<PagedResult<MedicalSupplyDto>>.Ok(result);
    }
}
