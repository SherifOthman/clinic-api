using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalSupplies.Queries.GetMedicalSupplies;

public class GetMedicalSuppliesQueryHandler : IRequestHandler<GetMedicalSuppliesQuery, Result<PagedResult<MedicalSupplyDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicalSuppliesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<MedicalSupplyDto>>> Handle(GetMedicalSuppliesQuery request, CancellationToken cancellationToken)
    {
        PagedResult<MedicalSupply> supplies;
        
        if (request.PaginationRequest != null)
        {
            supplies = await _unitOfWork.MedicalSupplies.GetByClinicBranchIdPagedAsync(
                request.ClinicBranchId, 
                request.PaginationRequest, 
                cancellationToken);
        }
        else
        {
            var allSupplies = await _unitOfWork.MedicalSupplies.GetByClinicBranchIdAsync(request.ClinicBranchId, cancellationToken);
            supplies = new PagedResult<MedicalSupply>(allSupplies.ToList(), allSupplies.Count(), 1, allSupplies.Count());
        }
        
        var suppliesDto = supplies.Items.Adapt<List<MedicalSupplyDto>>();
        var result = new PagedResult<MedicalSupplyDto>(suppliesDto, supplies.TotalCount, supplies.PageNumber, supplies.PageSize);
        
        return Result<PagedResult<MedicalSupplyDto>>.Success(result);
    }
}