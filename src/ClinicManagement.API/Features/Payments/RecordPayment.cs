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
            // Generate payment ID
            var paymentId = Guid.NewGuid();

            // Use domain method - this handles all business logic
            invoice.AddPayment(
                paymentId,
                request.Amount,
                request.PaymentMethod,
                request.ReferenceNumber);

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Payment recorded: {PaymentId} Invoice={InvoiceId} Amount={Amount} Method={PaymentMethod} Reference={ReferenceNumber} by {UserId}",
                paymentId, invoiceId, request.Amount, request.PaymentMethod, request.ReferenceNumber, currentUser.UserId);

            // Load payment response
            var payment = await db.Payments
                .Where(p => p.Id == paymentId)
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
            logger.LogError(ex,
                "Failed to record payment Invoice={InvoiceId} Amount={Amount} by {UserId}",
                invoiceId, request.Amount, currentUser.UserId);
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
        decimal Amount,
        
        [Required]
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Invalid payment method")]
        PaymentMethod PaymentMethod,
        
        [MaxLength(100, ErrorMessage = "Reference number must not exceed 100 characters")]
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
