using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Invoices;

public class CancelInvoiceEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/invoices/{id:guid}/cancel", HandleAsync)
            .RequireAuthorization()
            .WithName("CancelInvoice")
            .WithSummary("Cancel an invoice")
            .WithTags("Invoices")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        ILogger<CancelInvoiceEndpoint> logger,
        CancellationToken ct)
    {
        // Load invoice - ClinicId filter is automatic via global query filter
        var invoice = await db.Invoices
            .Where(i => i.Id == id)
            .FirstOrDefaultAsync(ct);

        if (invoice is null)
            return Results.NotFound(new { error = "Invoice not found", code = "NOT_FOUND" });

        try
        {
            var previousStatus = invoice.Status;
            var invoiceNumber = invoice.InvoiceNumber;
            
            // Update status
            invoice.Status = InvoiceStatus.Cancelled;
            if (!string.IsNullOrEmpty(request.Reason))
            {
                invoice.Notes = string.IsNullOrEmpty(invoice.Notes) 
                    ? $"Cancelled: {request.Reason}" 
                    : $"{invoice.Notes}\nCancelled: {request.Reason}";
            }

            await db.SaveChangesAsync(ct);

            logger.LogWarning(
                "Invoice cancelled: {InvoiceId} {InvoiceNumber} Status={PreviousStatus}->{NewStatus} Reason={Reason} by {UserId}",
                id, invoiceNumber, previousStatus, invoice.Status, request.Reason, currentUser.UserId);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [MaxLength(255)]
        string? Reason);
}
