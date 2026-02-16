using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Invoices;

public class GetInvoiceByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/invoices/{id:guid}", HandleAsync)
            .RequireAuthorization()
            .WithName("GetInvoiceById")
            .WithSummary("Get invoice details by ID")
            .WithTags("Invoices")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // ClinicId filter is automatic via global query filter
        var invoice = await db.Invoices
            .Where(i => i.Id == id)
            .Select(i => new Response(
                i.Id,
                i.InvoiceNumber,
                i.PatientId,
                i.Patient.FullName,
                i.AppointmentId,
                i.Items.Select(item => new InvoiceItemDetail(
                    item.Id,
                    item.MedicalServiceId != null ? item.MedicalService!.Name :
                    item.MedicineId != null ? item.Medicine!.Name :
                    item.MedicalSupply!.Name,
                    item.MedicalServiceId != null ? "Service" :
                    item.MedicineId != null ? "Medicine" : "Supply",
                    item.Quantity,
                    item.UnitPrice,
                    item.LineTotal,
                    item.SaleUnit
                )).ToList(),
                i.SubtotalAmount,
                i.Discount,
                i.DiscountPercentage,
                i.TaxAmount,
                i.FinalAmount,
                i.TotalPaid,
                i.RemainingAmount,
                i.IsFullyPaid,
                i.IsPartiallyPaid,
                i.IsOverdue,
                i.DaysOverdue,
                i.Status,
                i.IssuedDate,
                i.DueDate,
                i.Notes,
                i.CreatedAt,
                i.UpdatedAt
            ))
            .FirstOrDefaultAsync(ct);

        return invoice is null
            ? Results.NotFound(new { error = "Invoice not found", code = "NOT_FOUND" })
            : Results.Ok(invoice);
    }

    public record Response(
        Guid Id,
        string InvoiceNumber,
        Guid PatientId,
        string PatientName,
        Guid? AppointmentId,
        List<InvoiceItemDetail> Items,
        decimal SubtotalAmount,
        decimal Discount,
        decimal DiscountPercentage,
        decimal TaxAmount,
        decimal FinalAmount,
        decimal TotalPaid,
        decimal RemainingAmount,
        bool IsFullyPaid,
        bool IsPartiallyPaid,
        bool IsOverdue,
        int DaysOverdue,
        InvoiceStatus Status,
        DateTime? IssuedDate,
        DateTime? DueDate,
        string? Notes,
        DateTime CreatedAt,
        DateTime? UpdatedAt);

    public record InvoiceItemDetail(
        Guid Id,
        string ItemName,
        string ItemType,
        int Quantity,
        decimal UnitPrice,
        decimal LineTotal,
        SaleUnit? SaleUnit);
}
