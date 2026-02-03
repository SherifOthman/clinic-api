using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Payments.Commands.CreatePayment;

public record CreatePaymentCommand : IRequest<Result<Guid>>
{
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public DateTime PaymentDate { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? Note { get; init; }
    public string? ReferenceNumber { get; init; }
}