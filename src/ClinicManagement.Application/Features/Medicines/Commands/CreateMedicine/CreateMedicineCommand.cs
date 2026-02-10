using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;

/// <summary>
/// Command to create a new medicine with comprehensive inventory management
/// </summary>
public record CreateMedicineCommand : IRequest<Result<Guid>>
{
    public Guid ClinicBranchId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? Manufacturer { get; init; }
    public string? BatchNumber { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public decimal BoxPrice { get; init; }
    public int StripsPerBox { get; init; }
    public int TotalStripsInStock { get; init; }
    public int MinimumStockLevel { get; init; } = 10;
    public int? ReorderLevel { get; init; }
}


/// <summary>
/// Handler for creating new medicines with comprehensive logging and validation
/// </summary>
public class CreateMedicineCommandHandler : IRequestHandler<CreateMedicineCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateMedicineCommandHandler> _logger;

    public CreateMedicineCommandHandler(IApplicationDbContext context, ILogger<CreateMedicineCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating medicine '{MedicineName}' for clinic branch {ClinicBranchId}", 
            request.Name, request.ClinicBranchId);

        try
        {
            // Check if clinic branch exists
            var clinicBranchExists = await _context.ClinicBranches
                .AnyAsync(cb => cb.Id == request.ClinicBranchId, cancellationToken);
            
            if (!clinicBranchExists)
            {
                _logger.LogWarning("Attempted to create medicine for non-existent clinic branch {ClinicBranchId}", 
                    request.ClinicBranchId);
                return Result<Guid>.Fail(MessageCodes.Common.CLINIC_BRANCH_NOT_FOUND);
            }

            // Check if medicine with same name already exists in this clinic branch
            var existingMedicine = await _context.Medicines
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Name.ToLower() == request.Name.ToLower() && m.ClinicBranchId == request.ClinicBranchId, cancellationToken);
            
            if (existingMedicine != null)
            {
                _logger.LogWarning("Medicine '{MedicineName}' already exists in clinic branch {ClinicBranchId}", 
                    request.Name, request.ClinicBranchId);
                return Result<Guid>.FailField("name", MessageCodes.Medicine.ALREADY_EXISTS);
            }

            // Create medicine entity
            var medicine = new Medicine
            {
                ClinicBranchId = request.ClinicBranchId,
                Name = request.Name,
                Description = request.Description,
                Manufacturer = request.Manufacturer,
                BoxPrice = request.BoxPrice,
                StripsPerBox = request.StripsPerBox,
                TotalStripsInStock = request.TotalStripsInStock,
                MinimumStockLevel = request.MinimumStockLevel,
                ReorderLevel = request.ReorderLevel ?? request.MinimumStockLevel * 2, // Default reorder level
                ExpiryDate = request.ExpiryDate,
                BatchNumber = request.BatchNumber
            };

            // Validate business rules
            medicine.Validate();
            
            _logger.LogDebug("Medicine validation passed. Box price: {BoxPrice}, Strips per box: {StripsPerBox}, Stock: {Stock}", 
                medicine.BoxPrice, medicine.StripsPerBox, medicine.TotalStripsInStock);

            // Check for low stock warning
            if (medicine.IsLowStock)
            {
                _logger.LogWarning("Medicine '{MedicineName}' created with low stock. Current: {CurrentStock}, Minimum: {MinimumStock}", 
                    medicine.Name, medicine.TotalStripsInStock, medicine.MinimumStockLevel);
            }

            // Check for expiry warning
            if (medicine.IsExpiringSoon)
            {
                _logger.LogWarning("Medicine '{MedicineName}' is expiring soon. Expiry date: {ExpiryDate}", 
                    medicine.Name, medicine.ExpiryDate);
            }

            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created medicine '{MedicineName}' with ID {MedicineId}. Stock: {Stock} strips, Value: {Value:C}", 
                medicine.Name, medicine.Id, medicine.TotalStripsInStock, medicine.InventoryValue);

            return Result<Guid>.Ok(medicine.Id);
        }
        catch (InvalidBusinessOperationException ex)
        {
            _logger.LogWarning("Invalid business operation for medicine creation: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return Result<Guid>.Fail(ex.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating medicine '{MedicineName}' for clinic branch {ClinicBranchId}", 
                request.Name, request.ClinicBranchId);
            throw;
        }
    }
}