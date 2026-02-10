using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
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
            .GreaterThan(0).WithErrorCode(MessageCodes.Invoice.INVALID_ITEM_QUANTITY);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithErrorCode(MessageCodes.Invoice.INVALID_ITEM_PRICE);

        RuleFor(x => x)
            .Must(item => item.MedicalServiceId.HasValue || item.MedicineId.HasValue || item.MedicalSupplyId.HasValue)
            .WithErrorCode("INVOICE.ITEM.REQUIRED");
    }
}

/// <summary>
/// Handler for creating new invoices with comprehensive logging and validation
/// </summary>
public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;

    public CreateInvoiceCommandHandler(
        IApplicationDbContext context,
        ICodeGeneratorService codeGeneratorService,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _context = context;
        _codeGeneratorService = codeGeneratorService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating invoice for patient {PatientId} with {ItemCount} items", 
            request.PatientId, request.Items.Count);

        try
        {
            // Validate clinic patient exists
            var Patient = await _context.Patients.FindAsync(new object[] { request.PatientId }, cancellationToken);
            if (Patient == null)
            {
                _logger.LogWarning("Attempted to create invoice for non-existent patient {PatientId}", request.PatientId);
                return Result<Guid>.Fail(MessageCodes.Invoice.PATIENT_REQUIRED);
            }

            _logger.LogDebug("Found clinic patient {PatientId} for clinic {ClinicId}", 
                Patient.Id, Patient.ClinicId);

            // Generate invoice number
            var invoiceNumber = await _codeGeneratorService.GenerateInvoiceNumberAsync(Patient.ClinicId, cancellationToken);
            _logger.LogDebug("Generated invoice number: {InvoiceNumber}", invoiceNumber);

            // Create invoice entity
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                ClinicId = Patient.ClinicId,
                PatientId = request.PatientId,
                MedicalVisitId = request.MedicalVisitId,
                Discount = request.Discount,
                Status = InvoiceStatus.Draft,
                DueDate = request.DueDate,
                Notes = request.Notes
            };

            // Add invoice items with validation
            foreach (var itemCommand in request.Items)
            {
                _logger.LogDebug("Adding invoice item: Service={ServiceId}, Medicine={MedicineId}, Supply={SupplyId}, Qty={Quantity}", 
                    itemCommand.MedicalServiceId, itemCommand.MedicineId, itemCommand.MedicalSupplyId, itemCommand.Quantity);

                var item = new InvoiceItem
                {
                    MedicalServiceId = itemCommand.MedicalServiceId,
                    MedicineId = itemCommand.MedicineId,
                    MedicalSupplyId = itemCommand.MedicalSupplyId,
                    Quantity = itemCommand.Quantity,
                    UnitPrice = itemCommand.UnitPrice,
                    SaleUnit = itemCommand.SaleUnit
                };

                invoice.AddItem(item);
            }

            // Validate business rules
            invoice.Validate();
            _logger.LogDebug("Invoice validation passed. Total amount: {TotalAmount}, Final amount: {FinalAmount}", 
                invoice.SubtotalAmount, invoice.FinalAmount);

            // Save to database
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created invoice {InvoiceNumber} with ID {InvoiceId} for patient {PatientId}. Amount: {Amount}", 
                invoice.InvoiceNumber, invoice.Id, request.PatientId, invoice.FinalAmount);

            return Result<Guid>.Ok(invoice.Id);
        }
        catch (InvalidDiscountException ex)
        {
            _logger.LogWarning("Invalid discount for invoice: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return Result<Guid>.Fail(ex.ErrorCode);
        }
        catch (InvalidInvoiceStateException ex)
        {
            _logger.LogWarning("Invalid invoice state operation: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return Result<Guid>.Fail(ex.ErrorCode);
        }
        catch (InvalidBusinessOperationException ex)
        {
            _logger.LogWarning("Invalid business operation: {ErrorCode} - {Message}", ex.ErrorCode, ex.Message);
            return Result<Guid>.Fail(ex.ErrorCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for patient {PatientId}", request.PatientId);
            throw;
        }
    }
}