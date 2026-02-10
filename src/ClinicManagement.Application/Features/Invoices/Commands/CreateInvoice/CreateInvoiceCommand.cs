using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;

public record CreateInvoiceCommand : IRequest<Result<Guid>>
{
    public Guid PatientId { get; init; }
    public Guid? MedicalVisitId { get; init; }
    public decimal Discount { get; init; }
    public DateTime? DueDate { get; init; }
    public string? Notes { get; init; }
    public List<CreateInvoiceItemCommand> Items { get; init; } = new();
}

public record CreateInvoiceItemCommand
{
    public Guid? MedicalServiceId { get; init; }
    public Guid? MedicineId { get; init; }
    public Guid? MedicalSupplyId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public SaleUnit? SaleUnit { get; init; }
}


public class CreateInvoiceItemCommandValidator : AbstractValidator<CreateInvoiceItemCommand>
{
    public CreateInvoiceItemCommandValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0");

        RuleFor(x => x)
            .Must(item => item.MedicalServiceId.HasValue || item.MedicineId.HasValue || item.MedicalSupplyId.HasValue)
            .WithMessage("At least one item type (service, medicine, or supply) must be specified");
    }
}

/// <summary>
/// Handler for creating new invoices with comprehensive logging and validation
/// </summary>
public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;

    public CreateInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        ICodeGeneratorService codeGeneratorService,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _codeGeneratorService = codeGeneratorService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating invoice for patient {PatientId} with {ItemCount} items", 
            request.PatientId, request.Items.Count);

        try
        {
            // Validate patient exists
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.PatientId, cancellationToken);
            if (patient == null)
            {
                _logger.LogWarning("Attempted to create invoice for non-existent patient {PatientId}", request.PatientId);
                return Result<Guid>.FailSystem("NOT_FOUND", "Patient not found");
            }

            _logger.LogDebug("Found patient {PatientId} for clinic {ClinicId}", 
                patient.Id, patient.ClinicId);

            // Generate invoice number
            var invoiceNumber = await _codeGeneratorService.GenerateInvoiceNumberAsync(patient.ClinicId, cancellationToken);
            _logger.LogDebug("Generated invoice number: {InvoiceNumber}", invoiceNumber);

            // Create invoice using factory method
            var invoice = Invoice.Create(
                invoiceNumber,
                patient.ClinicId,
                request.PatientId,
                appointmentId: null,  // No appointment link for now
                medicalVisitId: request.MedicalVisitId,
                dueDate: request.DueDate);

            _logger.LogDebug("Created invoice with number {InvoiceNumber}", invoiceNumber);

            // Add invoice items using aggregate method
            foreach (var itemCommand in request.Items)
            {
                _logger.LogDebug("Adding invoice item: Service={ServiceId}, Medicine={MedicineId}, Supply={SupplyId}, Qty={Quantity}", 
                    itemCommand.MedicalServiceId, itemCommand.MedicineId, itemCommand.MedicalSupplyId, itemCommand.Quantity);

                invoice.AddItem(
                    medicalServiceId: itemCommand.MedicalServiceId,
                    medicineId: itemCommand.MedicineId,
                    medicalSupplyId: itemCommand.MedicalSupplyId,
                    quantity: itemCommand.Quantity,
                    unitPrice: itemCommand.UnitPrice,
                    saleUnit: itemCommand.SaleUnit);
            }

            // Apply discount if provided
            if (request.Discount > 0)
            {
                invoice.ApplyDiscount(request.Discount);
                _logger.LogDebug("Applied discount of {Discount} to invoice", request.Discount);
            }

            // Update notes if provided
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                invoice.UpdateNotes(request.Notes);
            }

            _logger.LogDebug("Invoice validation passed. Total amount: {TotalAmount}, Final amount: {FinalAmount}", 
                invoice.SubtotalAmount, invoice.FinalAmount);

            // Save to database
            await _unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created invoice {InvoiceNumber} with ID {InvoiceId} for patient {PatientId}. Amount: {Amount}", 
                invoice.InvoiceNumber, invoice.Id, request.PatientId, invoice.FinalAmount);

            return Result<Guid>.Ok(invoice.Id);
        }
        catch (InvalidDiscountException ex)
        {
            _logger.LogWarning("Invalid discount for invoice: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return Result<Guid>.FailBusiness("INVALID_DISCOUNT", ex.Message);
        }
        catch (InvalidInvoiceStateException ex)
        {
            _logger.LogWarning("Invalid invoice state operation: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return Result<Guid>.FailBusiness("INVALID_INVOICE_STATE", ex.Message);
        }
        catch (InvalidBusinessOperationException ex)
        {
            _logger.LogWarning("Invalid business operation: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return Result<Guid>.FailBusiness("INVALID_OPERATION", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for patient {PatientId}", request.PatientId);
            return Result<Guid>.FailSystem("INTERNAL_ERROR", "An error occurred while creating the invoice");
        }
    }
}