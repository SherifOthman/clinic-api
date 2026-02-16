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
        ILogger<CancelAppointmentEndpoint> logger,
        CancellationToken ct)
    {
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
            var previousStatus = appointment.Status;
            
            // Update status
            appointment.Status = AppointmentStatus.Cancelled;
            
            await db.SaveChangesAsync(ct);
            
            logger.LogWarning(
                "Appointment cancelled: {AppointmentId} Patient={PatientId} Status={PreviousStatus}->{NewStatus} Reason={Reason} by {UserId}",
                id, appointment.PatientId, previousStatus, appointment.Status, request.Reason, currentUser.UserId);
            
            return Results.Ok(new MessageResponse("Appointment cancelled successfully"));
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
