using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.PatientChronicDiseases;

public class AddPatientChronicDiseaseEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/patients/{patientId:guid}/chronic-diseases", HandleAsync)
            .RequireAuthorization()
            .WithName("AddPatientChronicDisease")
            .WithSummary("Add a chronic disease to a patient")
            .WithTags("PatientChronicDiseases")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Guid patientId,
        Request request,
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

        // Verify chronic disease exists
        var diseaseExists = await db.ChronicDiseases
            .AnyAsync(cd => cd.Id == request.ChronicDiseaseId, ct);

        if (!diseaseExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.DISEASE_NOT_FOUND,
                Title = "Disease Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Chronic disease not found"
            });

        // Check if patient already has this disease
        var alreadyExists = await db.PatientChronicDiseases
            .AnyAsync(pcd =>
                pcd.PatientId == patientId &&
                pcd.ChronicDiseaseId == request.ChronicDiseaseId, ct);

        if (alreadyExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.DISEASE_ALREADY_EXISTS,
                Title = "Disease Already Exists",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Patient already has this chronic disease"
            });

        var patientDisease = new PatientChronicDisease
        {
            PatientId = patientId,
            ChronicDiseaseId = request.ChronicDiseaseId
        };

        db.PatientChronicDiseases.Add(patientDisease);
        await db.SaveChangesAsync(ct);

        // Load the disease name for response
        var diseaseName = await db.ChronicDiseases
            .Where(cd => cd.Id == request.ChronicDiseaseId)
            .Select(cd => cd.NameEn)
            .FirstAsync(ct);

        var response = new Response(
            request.ChronicDiseaseId,
            diseaseName);

        return Results.Created($"/api/patients/{patientId}/chronic-diseases/{request.ChronicDiseaseId}", response);
    }

    public record Request(
        [Required]
        Guid ChronicDiseaseId);

    public record Response(
        Guid ChronicDiseaseId,
        string ChronicDiseaseName);
}
