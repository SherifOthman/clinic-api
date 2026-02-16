using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Payments;

public class RecordPaymentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/invoices/{invoiceId:guid}/payments", HandleAsync)
            .RequireAuthorization()
            .WithName("RecordPayment")
            .WithSummary("Record a payment for an invoice")
            .WithTags("Payments")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Guid invoiceId,
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        DateTimeProvider dateTimeProvider,
        ILogger<RecordPaymentEndpoint> logger,
        CancellationToken ct)
    {
        // Load invoice - ClinicId filter is automatic via global query filter
        var invoice = await db.Invoices
            .Where(i => i.Id == invoiceId)
            .FirstOrDefaultAsync(ct);

        if (invoice is null)
            return Results.NotFound(new { error = "Invoice not found", code = "NOT_FOUND" });

        try
        {
            var payment = new Payment
            {
                InvoiceId = invoiceId,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                ReferenceNumber = request.ReferenceNumber,
                PaymentDate = dateTimeProvider.UtcNow,
                Status = PaymentStatus.Paid
            };

            db.Payments.Add(payment);

            // Update invoice status
            var totalPaid = await db.Payments
                .Where(p => p.InvoiceId == invoiceId && p.Status == PaymentStatus.Paid)
                .SumAsync(p => p.Amount, ct) + request.Amount;

            if (totalPaid >= invoice.FinalAmount)
            {
                invoice.Status = InvoiceStatus.FullyPaid;
            }
            else if (totalPaid > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Payment recorded: {PaymentId} Invoice={InvoiceId} Amount={Amount} Method={PaymentMethod} Reference={ReferenceNumber} by {UserId}",
                payment.Id, invoiceId, request.Amount, request.PaymentMethod, request.ReferenceNumber, currentUser.UserId);

            // Load payment response
            var paymentResponse = await db.Payments
                .Where(p => p.Id == payment.Id)
                .Select(p => new Response(
                    p.Id,
                    p.InvoiceId,
                    p.Invoice.InvoiceNumber,
                    p.Amount,
                    p.PaymentMethod,
                    p.ReferenceNumber,
                    p.PaymentDate,
                    p.Invoice.RemainingAmount,
                    p.Invoice.IsFullyPaid,
                    p.Status
                ))
                .FirstAsync(ct);

            return Results.Created($"/api/payments/{payment.Id}", payment);
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        [Range(0.01, double.MaxValue)]
        decimal Amount,
        
        [Required]
        [EnumDataType(typeof(PaymentMethod))]
        PaymentMethod PaymentMethod,
        
        [MaxLength(50)]
        string? ReferenceNumber);

    public record Response(
        Guid Id,
        Guid InvoiceId,
        string InvoiceNumber,
        decimal Amount,
        PaymentMethod PaymentMethod,
        string? ReferenceNumber,
        DateTime PaymentDate,
        decimal RemainingAmount,
        bool IsInvoiceFullyPaid,
        PaymentStatus Status);
}
