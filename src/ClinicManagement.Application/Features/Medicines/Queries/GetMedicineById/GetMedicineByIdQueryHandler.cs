using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Queries.GetMedicineById;

public record GetMedicineByIdQuery(Guid Id) : IRequest<Result<MedicineDto>>;

public class GetMedicineByIdQueryHandler : IRequestHandler<GetMedicineByIdQuery, Result<MedicineDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMedicineByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MedicineDto>> Handle(GetMedicineByIdQuery request, CancellationToken cancellationToken)
    {
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(request.Id, cancellationToken);
        
        if (medicine == null)
        {
            return Result<MedicineDto>.Fail(MessageCodes.Medicine.NOT_FOUND);
        }

        var medicineDto = medicine.Adapt<MedicineDto>();
        return Result<MedicineDto>.Ok(medicineDto);
    }
}
