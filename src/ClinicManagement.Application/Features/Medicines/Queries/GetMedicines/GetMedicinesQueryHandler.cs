using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;

public class GetMedicinesQueryHandler : IRequestHandler<GetMedicinesQuery, Result<IEnumerable<MedicineDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicinesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<MedicineDto>>> Handle(GetMedicinesQuery request, CancellationToken cancellationToken)
    {
        var medicines = await _unitOfWork.Medicines.GetByClinicIdAsync(request.ClinicBranchId, cancellationToken);
        var medicinesDto = medicines.Adapt<IEnumerable<MedicineDto>>();
        
        return Result<IEnumerable<MedicineDto>>.Success(medicinesDto);
    }
}