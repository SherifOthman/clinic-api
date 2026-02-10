using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
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
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicinesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<MedicineDto>>> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        PagedResult<MedicineDto> result;
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            // Paginated result
            var paginationRequest = new PaginationRequest 
            { 
                PageNumber = request.PageNumber.Value, 
                PageSize = request.PageSize.Value 
            };
            
            var pagedResult = await _unitOfWork.Medicines.GetPagedByClinicBranchAsync(
                request.ClinicBranchId, 
                paginationRequest, 
                cancellationToken);
            
            var medicinesDto = pagedResult.Items.Adapt<List<MedicineDto>>();
            result = new PagedResult<MedicineDto>(medicinesDto, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
        }
        else
        {
            // Return all items as a single page
            var allMedicines = await _unitOfWork.Medicines.GetByClinicBranchAsync(request.ClinicBranchId, cancellationToken);
            var medicinesDto = allMedicines.Adapt<List<MedicineDto>>();
            result = new PagedResult<MedicineDto>(medicinesDto, medicinesDto.Count, 1, medicinesDto.Count);
        }
        
        return Result<PagedResult<MedicineDto>>.Ok(result);
    }
}
