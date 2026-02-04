using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

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
