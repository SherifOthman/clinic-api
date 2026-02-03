using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;

public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, Result<PagedResult<MedicineDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicinesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<MedicineDto>>> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        PagedResult<Medicine> medicines;
        
        if (request.PaginationRequest != null)
        {
            medicines = await _unitOfWork.Medicines.GetByClinicBranchIdPagedAsync(
                request.ClinicBranchId, 
                request.PaginationRequest, 
                cancellationToken);
        }
        else
        {
            // For backward compatibility, return all items as a single page
            var allMedicines = await _unitOfWork.Medicines.GetByClinicBranchIdAsync(request.ClinicBranchId, cancellationToken);
            medicines = new PagedResult<Medicine>(allMedicines.ToList(), allMedicines.Count(), 1, allMedicines.Count());
        }
        
        var medicinesDto = medicines.Items.Adapt<List<MedicineDto>>();
        var result = new PagedResult<MedicineDto>(medicinesDto, medicines.TotalCount, medicines.PageNumber, medicines.PageSize);
        
        return Result<PagedResult<MedicineDto>>.Success(result);
    }
}