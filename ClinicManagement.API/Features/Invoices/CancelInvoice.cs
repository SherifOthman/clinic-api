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
            
            // Use domain method
            invoice.Cancel(request.Reason);

            await db.SaveChangesAsync(ct);

            logger.LogWarning(
                "Invoice cancelled: {InvoiceId} {InvoiceNumber} Status={PreviousStatus}->{NewStatus} Reason={Reason} by {UserId}",
                id, invoiceNumber, previousStatus, invoice.Status, request.Reason, currentUser.UserId);

            return Results.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to cancel invoice {InvoiceId} by {UserId}",
                id, currentUser.UserId);
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [MaxLength(500, ErrorMessage = "Reason must not exceed 500 characters")]
        string? Reason);
}
