using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Payments;

public class GetInvoicePaymentsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/invoices/{invoiceId:guid}/payments", HandleAsync)
            .RequireAuthorization()
            .WithName("GetInvoicePayments")
            .WithSummary("List all payments for a specific invoice")
            .WithTags("Payments")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid invoiceId,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // ClinicId filter is automatic via global query filter
        var invoice = await db.Invoices
            .Where(i => i.Id == invoiceId)
            .Select(i => new Response(
                i.Id,
                i.InvoiceNumber,
                i.FinalAmount,
                i.TotalPaid,
                i.RemainingAmount,
                i.IsFullyPaid,
                i.Payments.Select(p => new PaymentListItem(
                    p.Id,
                    p.Amount,
                    p.PaymentMethod,
                    p.ReferenceNumber,
                    p.PaymentDate,
                    p.Status
                )).OrderByDescending(p => p.PaymentDate).ToList()
            ))
            .FirstOrDefaultAsync(ct);

        return invoice is null
            ? Results.NotFound(new { error = "Invoice not found", code = "NOT_FOUND" })
            : Results.Ok(invoice);
    }

    public record Response(
        Guid InvoiceId,
        string InvoiceNumber,
        decimal InvoiceFinalAmount,
        decimal TotalPaid,
        decimal RemainingAmount,
        bool IsFullyPaid,
        List<PaymentListItem> Payments);

    public record PaymentListItem(
        Guid Id,
        decimal Amount,
        PaymentMethod PaymentMethod,
        string? ReferenceNumber,
        DateTime PaymentDate,
        PaymentStatus Status);
}
