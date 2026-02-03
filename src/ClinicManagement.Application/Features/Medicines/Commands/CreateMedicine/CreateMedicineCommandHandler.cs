using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;

public class CreateMedicineCommandHandler : IRequestHandler<CreateMedicineCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateMedicineCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        var medicine = new Medicine
        {
            Id = Guid.NewGuid(),
            ClinicBranchId = request.ClinicBranchId,
            Name = request.Name,
            BoxPrice = request.BoxPrice,
            StripsPerBox = request.StripsPerBox,
            TotalStripsInStock = request.TotalStripsInStock,
            MinimumStockLevel = request.MinimumStockLevel
        };

        await _unitOfWork.Medicines.AddAsync(medicine, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(medicine.Id);
    }
}