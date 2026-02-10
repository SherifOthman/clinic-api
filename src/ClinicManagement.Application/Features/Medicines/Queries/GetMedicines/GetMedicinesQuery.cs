using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;

public record GetMedicinesQuery(
    Guid ClinicBranchId,
    int? PageNumber = null,
    int? PageSize = null
) : IRequest<Result<PagedResult<MedicineDto>>>;

public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, Result<PagedResult<MedicineDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMedicinesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<MedicineDto>>> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Medicines
            .AsNoTracking()
            .Where(m => m.ClinicBranchId == request.ClinicBranchId)
            .OrderBy(m => m.Name);

        PagedResult<MedicineDto> result;
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            // Paginated result
            var totalCount = await query.CountAsync(cancellationToken);
            
            var medicines = await query
                .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value)
                .ToListAsync(cancellationToken);
            
            var medicinesDto = medicines.Adapt<List<MedicineDto>>();
            result = new PagedResult<MedicineDto>(medicinesDto, totalCount, request.PageNumber.Value, request.PageSize.Value);
        }
        else
        {
            // Return all items as a single page
            var allMedicines = await query.ToListAsync(cancellationToken);
            var medicinesDto = allMedicines.Adapt<List<MedicineDto>>();
            result = new PagedResult<MedicineDto>(medicinesDto, medicinesDto.Count, 1, medicinesDto.Count);
        }
        
        return Result<PagedResult<MedicineDto>>.Ok(result);
    }
}
