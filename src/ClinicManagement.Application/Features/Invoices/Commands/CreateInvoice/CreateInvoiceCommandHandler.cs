using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateInvoiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ClinicId = request.ClinicId,
            ClinicPatientId = request.ClinicPatientId,
            MedicalVisitId = request.MedicalVisitId,
            Discount = request.Discount,
            Status = InvoiceStatus.Draft,
            DueDate = request.DueDate
        };

        // Add invoice items
        foreach (var itemCommand in request.Items)
        {
            var item = new InvoiceItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                MedicalServiceId = itemCommand.MedicalServiceId,
                MedicineId = itemCommand.MedicineId,
                MedicalSupplyId = itemCommand.MedicalSupplyId,
                Quantity = itemCommand.Quantity,
                UnitPrice = itemCommand.UnitPrice,
                SaleUnit = itemCommand.SaleUnit
            };

            invoice.Items.Add(item);
        }

        // Calculate total amount
        invoice.TotalAmount = invoice.Items.Sum(i => i.Quantity * i.UnitPrice);

        await _unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(invoice.Id);
    }
}