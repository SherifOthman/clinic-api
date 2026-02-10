using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.DeleteMedicine;

public record DeleteMedicineCommand(Guid Id) : IRequest<Result>;

public class DeleteMedicineCommandHandler : IRequestHandler<DeleteMedicineCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMedicineCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
    {
        var medicine = await _unitOfWork.Medicines.GetByIdAsync(request.Id, cancellationToken);
        
        if (medicine == null)
        {
            return Result.Fail("Medicine not found.");
        }

        _unitOfWork.Medicines.Delete(medicine);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}