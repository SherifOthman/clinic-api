using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Validation;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Patients;

public class UpdatePatientEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/patients/{id:guid}", HandleAsync)
            .RequireAuthorization("StaffAccess")
            .WithName("UpdatePatient")
            .WithSummary("Update patient basic information")
            .WithTags("Patients")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        ILogger<UpdatePatientEndpoint> logger,
        CancellationToken ct)
    {
        // Find patient - ClinicId filter is automatic via global query filter
        var patient = await db.Patients
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync(ct);

        if (patient is null)
            return Results.NotFound(new { error = "Patient not found", code = "NOT_FOUND" });

        try
        {
            // Update patient info
            patient.FullName = request.FullName;
            patient.IsMale = request.IsMale;
            patient.DateOfBirth = request.DateOfBirth;
            patient.CityGeoNameId = request.CityGeoNameId;

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Patient updated: {PatientId} {PatientCode} {PatientName} by {UserId}",
                id, patient.PatientCode, patient.FullName, currentUser.UserId);

            // Load updated patient with related data
            var updatedPatient = await db.Patients
                .Where(p => p.Id == patient.Id)
                .Select(p => new Response(
                    p.Id,
                    p.PatientCode,
                    p.FullName,
                    p.IsMale,
                    p.DateOfBirth,
                    p.DateOfBirth.CalculateAge(),
                    p.CityGeoNameId,
                    p.PhoneNumbers
                        .Where(pn => pn.IsPrimary)
                        .Select(pn => pn.PhoneNumber)
                        .FirstOrDefault(),
                    p.PhoneNumbers
                        .Select(pn => new PhoneNumberInfo(pn.PhoneNumber, pn.IsPrimary))
                        .ToList(),
                    p.ChronicDiseases
                        .Select(cd => new ChronicDisease(cd.ChronicDiseaseId, cd.ChronicDisease.NameEn))
                        .ToList(),
                    p.CreatedAt
                ))
                .FirstAsync(ct);

            return Results.Ok(updatedPatient);
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        [MaxLength(100)]
        string FullName,
        
        [Required]
        bool IsMale,
        
        [Required]
        [CustomValidation(typeof(CustomValidators), nameof(CustomValidators.MustBeInPast))]
        DateTime DateOfBirth,
        
        int? CityGeoNameId);

    public record Response(
        Guid Id,
        string PatientCode,
        string FullName,
        bool IsMale,
        DateTime DateOfBirth,
        int Age,
        int? CityGeoNameId,
        string? PrimaryPhone,
        List<PhoneNumberInfo> PhoneNumbers,
        List<ChronicDisease> ChronicDiseases,
        DateTime CreatedAt);

    public record PhoneNumberInfo(string PhoneNumber, bool IsPrimary);
    public record ChronicDisease(Guid Id, string Name);
}
