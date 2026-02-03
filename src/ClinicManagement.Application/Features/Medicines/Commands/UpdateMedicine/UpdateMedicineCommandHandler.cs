using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.UpdateMedicine;

public class UpdateMedicineCommandHandler : IRequestHandler<UpdateMedicineCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMedicineCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateMedicineCommand request, CancellationToken cancellationToken)
    {
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(request.Id, cancellationToken);
        
        if (medicine == null)
        {
            return Result.Fail("Medicine not found.");
        }

        medicine.Name = request.Name;
        medicine.BoxPrice = request.BoxPrice;
        medicine.StripsPerBox = request.StripsPerBox;
        medicine.TotalStripsInStock = request.TotalStripsInStock;
        medicine.MinimumStockLevel = request.MinimumStockLevel;

        _unitOfWork.Medicines.Update(medicine);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
