using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Invoices;

public class GetPatientInvoicesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{patientId:guid}/invoices", HandleAsync)
            .RequireAuthorization()
            .WithName("GetPatientInvoices")
            .WithSummary("List invoices for a specific patient")
            .WithTags("Invoices")
            .Produces<PaginatedResult<InvoiceListItem>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        Guid patientId,
        [AsParameters] Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // Build query with filters - ClinicId filter is automatic via global query filter
        var query = db.Invoices
            .Where(i => i.PatientId == patientId)
            .WhereIf(request.Status.HasValue, i => i.Status == request.Status!.Value)
            .WhereIf(request.IsOverdue == true, i => i.IsOverdue);

        // Get total count
        var totalCount = await query.CountAsync(ct);

        // Apply pagination and sorting
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Paginate(request.Page, request.PageSize)
            .Select(i => new InvoiceListItem(
                i.Id,
                i.InvoiceNumber,
                i.PatientId,
                i.Patient.FullName,
                i.FinalAmount,
                i.TotalPaid,
                i.RemainingAmount,
                i.IsFullyPaid,
                i.IsOverdue,
                i.Status,
                i.IssuedDate,
                i.DueDate
            ))
            .ToListAsync(ct);

        return Results.Ok(new PaginatedResult<InvoiceListItem>(
            items,
            totalCount,
            request.Page,
            request.PageSize
        ));
    }

    public record Request(
        InvoiceStatus? Status = null,
        bool? IsOverdue = null,
        int Page = 1,
        int PageSize = 20);

    public record InvoiceListItem(
        Guid Id,
        string InvoiceNumber,
        Guid PatientId,
        string PatientName,
        decimal FinalAmount,
        decimal TotalPaid,
        decimal RemainingAmount,
        bool IsFullyPaid,
        bool IsOverdue,
        InvoiceStatus Status,
        DateTime? IssuedDate,
        DateTime? DueDate);
}
