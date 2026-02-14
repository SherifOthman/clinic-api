using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Appointments;

public class CompleteAppointmentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/appointments/{id:guid}/complete", HandleAsync)
            .RequireAuthorization()
            .WithName("CompleteAppointment")
            .WithSummary("Complete an appointment (Confirmed -> Completed)")
            .WithTags("Appointments")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        ILogger<CompleteAppointmentEndpoint> logger,
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
            appointment.Status = AppointmentStatus.Completed;
            
            await db.SaveChangesAsync(ct);
            
            logger.LogInformation(
                "Appointment completed: {AppointmentId} Patient={PatientId} Status={PreviousStatus}->{NewStatus} by {UserId}",
                id, appointment.PatientId, previousStatus, appointment.Status, currentUser.UserId);
            
            return Results.Ok(new MessageResponse("Appointment completed successfully"));
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }
}
