using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Appointments;

public class CancelAppointmentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/appointments/{id:guid}/cancel", HandleAsync)
            .RequireAuthorization()
            .WithName("CancelAppointment")
            .WithSummary("Cancel an appointment (Pending/Confirmed -> Cancelled)")
            .WithTags("Appointments")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        var clinicId = currentUser.ClinicId!.Value;

        // Load appointment
        // ClinicId filter is automatic via global query filter
        var appointment = await db.Appointments
            .Include(a => a.Patient)
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync(ct);

        if (appointment is null)
            return Results.NotFound(new { error = "Appointment not found", code = "NOT_FOUND" });

        try
        {
            // Domain method handles state transition and business rules
            appointment.Cancel(request.Reason ?? string.Empty);
            
            await db.SaveChangesAsync(ct);
            
            return Results.Ok(new { message = "Appointment cancelled successfully" });
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [MaxLength(500, ErrorMessage = "Reason must not exceed 500 characters")]
        string? Reason);
}
