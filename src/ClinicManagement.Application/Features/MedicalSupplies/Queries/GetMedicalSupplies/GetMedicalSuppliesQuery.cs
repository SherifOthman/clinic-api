using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
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
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicalSuppliesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<MedicalSupplyDto>>> Handle(GetMedicalSuppliesQuery request, CancellationToken cancellationToken)
    {
        PagedResult<MedicalSupplyDto> result;
        
        if (request.PageNumber.HasValue && request.PageSize.HasValue)
        {
            // Paginated result
            var paginationRequest = new PaginationRequest 
            { 
                PageNumber = request.PageNumber.Value, 
                PageSize = request.PageSize.Value 
            };
            
            var pagedResult = await _unitOfWork.MedicalSupplies.GetPagedByClinicBranchAsync(
                request.ClinicBranchId, 
                paginationRequest, 
                cancellationToken);
            
            var suppliesDto = pagedResult.Items.Adapt<List<MedicalSupplyDto>>();
            result = new PagedResult<MedicalSupplyDto>(suppliesDto, pagedResult.TotalCount, pagedResult.PageNumber, pagedResult.PageSize);
        }
        else
        {
            // Return all items as a single page
            var allSupplies = await _unitOfWork.MedicalSupplies.GetByClinicBranchAsync(request.ClinicBranchId, cancellationToken);
            var suppliesDto = allSupplies.Adapt<List<MedicalSupplyDto>>();
            result = new PagedResult<MedicalSupplyDto>(suppliesDto, suppliesDto.Count, 1, suppliesDto.Count);
        }
        
        return Result<PagedResult<MedicalSupplyDto>>.Ok(result);
    }
}
