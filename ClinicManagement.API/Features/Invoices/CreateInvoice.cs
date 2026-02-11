using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Validation;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Invoices;

public class CreateInvoiceEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/invoices", HandleAsync)
            .RequireAuthorization()
            .WithName("CreateInvoice")
            .WithSummary("Create a new invoice")
            .WithTags("Invoices")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CodeGeneratorService codeGenerator,
        CancellationToken ct)
    {
        // Verify patient exists (global query filter ensures it belongs to clinic)
        var patientExists = await db.Patients
            .AnyAsync(p => p.Id == request.PatientId, ct);

        if (!patientExists)
            return Results.BadRequest(new
            {
                error = "Patient not found or does not belong to your clinic",
                code = "PATIENT_NOT_FOUND"
            });

        // Verify appointment if provided (global query filter ensures it belongs to clinic)
        if (request.AppointmentId.HasValue)
        {
            var appointmentExists = await db.Appointments
                .AnyAsync(a => a.Id == request.AppointmentId.Value, ct);

            if (!appointmentExists)
                return Results.BadRequest(new
                {
                    error = "Appointment not found or does not belong to your clinic",
                    code = "APPOINTMENT_NOT_FOUND"
                });
        }

        // Generate invoice number
        var invoiceNumber = await codeGenerator.GenerateInvoiceNumberAsync(ct);

        try
        {
            // Use domain factory method
            var invoice = Invoice.Create(
                invoiceNumber,
                currentUser.ClinicId!.Value,
                request.PatientId,
                request.AppointmentId,
                null, // MedicalVisitId
                request.DueDate);

            // Add items using domain method
            foreach (var item in request.Items)
            {
                invoice.AddItem(
                    item.MedicalServiceId,
                    item.MedicineId,
                    item.MedicalSupplyId,
                    item.Quantity,
                    item.UnitPrice,
                    item.SaleUnit);
            }

            // Apply discount if provided
            if (request.Discount.HasValue && request.Discount.Value > 0)
            {
                invoice.ApplyDiscount(request.Discount.Value);
            }

            // Set tax if provided
            if (request.TaxAmount.HasValue && request.TaxAmount.Value > 0)
            {
                invoice.SetTax(request.TaxAmount.Value);
            }

            // Update notes if provided
            if (!string.IsNullOrEmpty(request.Notes))
            {
                invoice.UpdateNotes(request.Notes);
            }

            // Issue the invoice
            invoice.Issue();

            db.Invoices.Add(invoice);
            await db.SaveChangesAsync(ct);

            // Load response with related data
            var response = await db.Invoices
                .Where(i => i.Id == invoice.Id)
                .Select(i => new Response(
                    i.Id,
                    i.InvoiceNumber,
                    i.PatientId,
                    i.Patient.FullName,
                    i.AppointmentId,
                    i.SubtotalAmount,
                    i.Discount,
                    i.TaxAmount,
                    i.FinalAmount,
                    i.Status,
                    i.IssuedDate,
                    i.DueDate,
                    i.CreatedAt
                ))
                .FirstAsync(ct);

            return Results.Created($"/api/invoices/{response.Id}", response);
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        Guid PatientId,
        
        Guid? AppointmentId,
        
        [Required]
        [MinLength(1, ErrorMessage = "At least one invoice item is required")]
        List<InvoiceItemInput> Items,
        
        [Range(0, double.MaxValue, ErrorMessage = "Discount cannot be negative")]
        decimal? Discount,
        
        [Range(0, double.MaxValue, ErrorMessage = "Tax amount cannot be negative")]
        decimal? TaxAmount,
        
        [CustomValidation(typeof(CustomValidators), nameof(CustomValidators.MustBeInFutureOrNull))]
        DateTime? DueDate,
        
        string? Notes);

    public record InvoiceItemInput(
        Guid? MedicalServiceId,
        Guid? MedicineId,
        Guid? MedicalSupplyId,
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        int Quantity,
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        decimal UnitPrice,
        
        SaleUnit? SaleUnit);

    public record Response(
        Guid Id,
        string InvoiceNumber,
        Guid PatientId,
        string PatientName,
        Guid? AppointmentId,
        decimal SubtotalAmount,
        decimal Discount,
        decimal TaxAmount,
        decimal FinalAmount,
        InvoiceStatus Status,
        DateTime? IssuedDate,
        DateTime? DueDate,
        DateTime CreatedAt);
}
