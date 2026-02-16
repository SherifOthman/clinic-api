using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.PatientChronicDiseases;

public class GetPatientChronicDiseasesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{patientId:guid}/chronic-diseases", HandleAsync)
            .RequireAuthorization()
            .WithName("GetPatientChronicDiseases")
            .WithSummary("Get chronic diseases for a patient")
            .WithTags("PatientChronicDiseases")
            .Produces<List<Response>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Guid patientId,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // Verify patient exists (global query filter ensures it belongs to clinic)
        var patientExists = await db.Patients
            .AnyAsync(p => p.Id == patientId, ct);

        if (!patientExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.PATIENT_NOT_FOUND,
                Title = "Patient Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Patient not found or does not belong to your clinic"
            });

        var diseases = await db.PatientChronicDiseases
            .Where(pcd => pcd.PatientId == patientId)
            .OrderBy(pcd => pcd.ChronicDisease.NameEn)
            .Select(pcd => new Response(
                pcd.ChronicDiseaseId,
                pcd.ChronicDisease.NameEn
            ))
            .ToListAsync(ct);

        return Results.Ok(diseases);
    }

    public record Response(
        Guid ChronicDiseaseId,
        string ChronicDiseaseName);
}
