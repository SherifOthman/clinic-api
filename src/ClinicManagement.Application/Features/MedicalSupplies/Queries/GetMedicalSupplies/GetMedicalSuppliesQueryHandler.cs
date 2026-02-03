using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalSupplies.Queries.GetMedicalSupplies;

public class GetMedicalSuppliesQueryHandler : IRequestHandler<GetMedicalSuppliesQuery, Result<IEnumerable<MedicalSupplyDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicalSuppliesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<MedicalSupplyDto>>> Handle(GetMedicalSuppliesQuery request, CancellationToken cancellationToken)
    {
        var supplies = await _unitOfWork.MedicalSupplies.GetByClinicIdAsync(request.ClinicBranchId, cancellationToken);
        var suppliesDto = supplies.Adapt<IEnumerable<MedicalSupplyDto>>();
        
        return Result<IEnumerable<MedicalSupplyDto>>.Success(suppliesDto);
    }
}