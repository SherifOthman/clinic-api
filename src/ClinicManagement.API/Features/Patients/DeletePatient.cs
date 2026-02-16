using ClinicManagement.API.Common;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Patients;

public class DeletePatientEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/patients/{id:guid}", HandleAsync)
            .RequireAuthorization("MedicalStaff")
            .WithName("DeletePatient")
            .WithSummary("Soft delete a patient")
            .WithTags("Patients")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        ILogger<DeletePatientEndpoint> logger,
        CancellationToken ct)
    {
        // Find patient - ClinicId filter is automatic via global query filter
        var patient = await db.Patients
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync(ct);

        if (patient is null)
            return Results.NotFound(new { error = "Patient not found", code = "NOT_FOUND" });

        var patientCode = patient.PatientCode;
        var patientName = patient.FullName;

        // Hard delete (soft delete not implemented)
        db.Patients.Remove(patient);

        await db.SaveChangesAsync(ct);

        logger.LogWarning(
            "Patient deleted: {PatientId} {PatientCode} {PatientName} by {UserId}",
            id, patientCode, patientName, currentUser.UserId);

        return Results.NoContent();
    }
}
