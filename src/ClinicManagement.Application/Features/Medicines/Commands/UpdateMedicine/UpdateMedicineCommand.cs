using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Medicines.Commands.UpdateMedicine;

public record UpdateMedicineCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? Manufacturer { get; init; }
    public string? BatchNumber { get; init; }
    public decimal BoxPrice { get; init; }
    public int StripsPerBox { get; init; }
    public int MinimumStockLevel { get; init; }
    public int? ReorderLevel { get; init; }
}

public class UpdateMedicineCommandHandler : IRequestHandler<UpdateMedicineCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMedicineCommandHandler> _logger;

    public UpdateMedicineCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateMedicineCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateMedicineCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating medicine {MedicineId}", request.Id);

        try
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(request.Id, cancellationToken);
            
            if (medicine == null)
            {
                _logger.LogWarning("Medicine {MedicineId} not found", request.Id);
                return Result.FailSystem("NOT_FOUND", "Medicine not found");
            }

            // Use behavior method to update info
            medicine.UpdateInfo(
                name: request.Name,
                boxPrice: request.BoxPrice,
                stripsPerBox: request.StripsPerBox,
                minimumStockLevel: request.MinimumStockLevel,
                reorderLevel: request.ReorderLevel,
                description: request.Description,
                manufacturer: request.Manufacturer,
                batchNumber: request.BatchNumber
            );

            _unitOfWork.Medicines.Update(medicine);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated medicine {MedicineId}", request.Id);
            return Result.Ok();
        }
        catch (InvalidBusinessOperationException ex)
        {
            _logger.LogWarning("Invalid business operation for medicine update: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return Result.FailBusiness("INVALID_OPERATION", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating medicine {MedicineId}", request.Id);
            return Result.FailSystem("INTERNAL_ERROR", "An error occurred while updating medicine");
        }
    }
}