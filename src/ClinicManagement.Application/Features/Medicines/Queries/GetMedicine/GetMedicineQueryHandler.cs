using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicine;

public class GetMedicineQueryHandler : IRequestHandler<GetMedicineQuery, Result<MedicineDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicineQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MedicineDto>> Handle(GetMedicineQuery request, CancellationToken cancellationToken)
    {
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(request.Id, cancellationToken);
        
        if (medicine == null)
        {
            return Result<MedicineDto>.Failure("Medicine not found.");
        }

        var medicineDto = medicine.Adapt<MedicineDto>();
        return Result<MedicineDto>.Success(medicineDto);
    }
}