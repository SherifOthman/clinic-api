using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;

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
            request.ClinicPatientId, request.Items.Count);

        try
        {
            // Validate clinic patient exists
            var clinicPatient = await _unitOfWork.ClinicPatients.GetByIdAsync(request.ClinicPatientId, cancellationToken);
            if (clinicPatient == null)
            {
                _logger.LogWarning("Attempted to create invoice for non-existent patient {PatientId}", request.ClinicPatientId);
                return Result<Guid>.Fail(MessageCodes.Invoice.PATIENT_REQUIRED);
            }

            _logger.LogDebug("Found clinic patient {PatientId} for clinic {ClinicId}", 
                clinicPatient.Id, clinicPatient.ClinicId);

            // Generate invoice number
            var invoiceNumber = await _codeGeneratorService.GenerateInvoiceNumberAsync(clinicPatient.ClinicId, cancellationToken);
            _logger.LogDebug("Generated invoice number: {InvoiceNumber}", invoiceNumber);

            // Create invoice entity
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                ClinicId = clinicPatient.ClinicId,
                ClinicPatientId = request.ClinicPatientId,
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
            await _unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully created invoice {InvoiceNumber} with ID {InvoiceId} for patient {PatientId}. Amount: {Amount}", 
                invoice.InvoiceNumber, invoice.Id, request.ClinicPatientId, invoice.FinalAmount);

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
            _logger.LogError(ex, "Error creating invoice for patient {PatientId}", request.ClinicPatientId);
            throw;
        }
    }
}
