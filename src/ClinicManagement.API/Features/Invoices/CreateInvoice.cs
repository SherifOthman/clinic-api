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
        ILogger<CreateInvoiceEndpoint> logger,
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
            var invoice = new Invoice
            {
                InvoiceNumber = invoiceNumber,
                ClinicId = currentUser.ClinicId!.Value,
                PatientId = request.PatientId,
                AppointmentId = request.AppointmentId,
                DueDate = request.DueDate,
                Status = InvoiceStatus.Draft
            };

            db.Invoices.Add(invoice);

            // Add items
            foreach (var item in request.Items)
            {
                db.InvoiceItems.Add(new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    MedicalServiceId = item.MedicalServiceId,
                    MedicineId = item.MedicineId,
                    MedicalSupplyId = item.MedicalSupplyId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SaleUnit = item.SaleUnit
                });
            }

            // Apply discount if provided
            if (request.Discount.HasValue && request.Discount.Value > 0)
            {
                invoice.Discount = request.Discount.Value;
            }

            // Set tax if provided
            if (request.TaxAmount.HasValue && request.TaxAmount.Value > 0)
            {
                invoice.TaxAmount = request.TaxAmount.Value;
            }

            // Update notes if provided
            if (!string.IsNullOrEmpty(request.Notes))
            {
                invoice.Notes = request.Notes;
            }

            // Issue the invoice
            invoice.Status = InvoiceStatus.Issued;
            invoice.IssuedDate = DateTime.UtcNow;
            invoice.DueDate ??= DateTime.UtcNow.AddDays(30);

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Invoice created: {InvoiceId} {InvoiceNumber} Patient={PatientId} Amount={FinalAmount} Items={ItemCount} by {UserId}",
                invoice.Id, invoiceNumber, request.PatientId, invoice.FinalAmount, request.Items.Count, currentUser.UserId);

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
            logger.LogError(ex,
                "Failed to create invoice Patient={PatientId} by {UserId}",
                request.PatientId, currentUser.UserId);
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
