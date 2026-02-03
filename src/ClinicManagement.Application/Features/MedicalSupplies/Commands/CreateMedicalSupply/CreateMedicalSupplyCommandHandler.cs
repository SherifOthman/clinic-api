using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalSupplies.Commands.CreateMedicalSupply;

public class CreateMedicalSupplyCommandHandler : IRequestHandler<CreateMedicalSupplyCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateMedicalSupplyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMedicalSupplyCommand request, CancellationToken cancellationToken)
    {
        var supply = new MedicalSupply
        {
            Id = Guid.NewGuid(),
            ClinicBranchId = request.ClinicBranchId,
            Name = request.Name,
            UnitPrice = request.UnitPrice,
            QuantityInStock = request.QuantityInStock,
            MinimumStockLevel = request.MinimumStockLevel
        };

        await _unitOfWork.MedicalSupplies.AddAsync(supply, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(supply.Id);
    }
}