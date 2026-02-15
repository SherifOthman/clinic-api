using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Appointments;

public class GetAppointmentByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/appointments/{id:guid}", HandleAsync)
            .RequireAuthorization()
            .WithName("GetAppointmentById")
            .WithSummary("Get an appointment by ID")
            .WithTags("Appointments")
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
        var appointment = await db.Appointments
            .Where(a => a.Id == id)
            .Select(a => new Response(
                a.Id,
                a.PatientId,
                a.Patient.FullName,
                a.Patient.PatientCode,
                a.DoctorId,
                a.DoctorProfile.Staff.User.FirstName + " " + a.DoctorProfile.Staff.User.LastName,
                a.ClinicBranchId,
                a.ClinicBranch.Name,
                a.AppointmentTypeId,
                a.AppointmentType.NameEn,
                a.AppointmentDate,
                a.QueueNumber,
                a.Status,
                a.CreatedAt,
                a.UpdatedAt
            ))
            .FirstOrDefaultAsync(ct);

        return appointment is null
            ? Results.NotFound(new { error = "Appointment not found", code = "NOT_FOUND" })
            : Results.Ok(appointment);
    }

    public record Response(
        Guid Id,
        Guid PatientId,
        string PatientName,
        string PatientCode,
        Guid DoctorId,
        string DoctorName,
        Guid ClinicBranchId,
        string BranchName,
        Guid AppointmentTypeId,
        string AppointmentTypeName,
        DateTime AppointmentDate,
        short QueueNumber,
        AppointmentStatus Status,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
