using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Patients;

public class GetPatientByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{id:guid}", HandleAsync)
            .RequireAuthorization()
            .WithName("GetPatientById")
            .WithSummary("Get a patient by ID")
            .WithTags("Patients")
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
        var patient = await db.Patients
            .Where(p => p.Id == id)
            .Select(p => new Response(
                p.Id,
                p.PatientCode,
                p.FullName,
                p.Gender,
                p.DateOfBirth,
                p.DateOfBirth.CalculateAge(),
                p.CityGeoNameId,
                p.PhoneNumbers
                    .Where(pn => pn.IsPrimary)
                    .Select(pn => pn.PhoneNumber)
                    .FirstOrDefault(),
                p.PhoneNumbers
                    .Select(pn => new PhoneNumberDto(pn.PhoneNumber, pn.IsPrimary))
                    .ToList(),
                p.ChronicDiseases
                    .Select(cd => new ChronicDiseaseDto(cd.ChronicDiseaseId, cd.ChronicDisease.NameEn))
                    .ToList(),
                p.CreatedAt
            ))
            .FirstOrDefaultAsync(ct);

        return patient is null
            ? Results.NotFound(new { error = "Patient not found", code = "NOT_FOUND" })
            : Results.Ok(patient);
    }

    public record Response(
        Guid Id,
        string PatientCode,
        string FullName,
        Gender Gender,
        DateTime DateOfBirth,
        int Age,
        int? CityGeoNameId,
        string? PrimaryPhone,
        List<PhoneNumberDto> PhoneNumbers,
        List<ChronicDiseaseDto> ChronicDiseases,
        DateTime CreatedAt);

    public record PhoneNumberDto(string PhoneNumber, bool IsPrimary);
    public record ChronicDiseaseDto(Guid Id, string Name);
}
