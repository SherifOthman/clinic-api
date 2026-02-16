using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Validation;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Patients;

public class CreatePatientEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/patients", HandleAsync)
            .RequireAuthorization("StaffAccess")
            .WithName("CreatePatient")
            .WithSummary("Create a new patient")
            .WithTags("Patients")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CodeGeneratorService codeGenerator,
        ILogger<CreatePatientEndpoint> logger,
        CancellationToken ct)
    {
        // Generate unique patient code
        var patientCode = await codeGenerator.GeneratePatientNumberAsync(ct);

        // Ensure at least one primary phone
        var phoneNumbers = request.PhoneNumbers.ToList();
        if (!phoneNumbers.Any(p => p.IsPrimary) && phoneNumbers.Any())
        {
            phoneNumbers[0] = phoneNumbers[0] with { IsPrimary = true };
        }

        try
        {
            var patient = new Patient
            {
                PatientCode = patientCode,
                ClinicId = currentUser.ClinicId!.Value,
                FullName = request.FullName,
                IsMale = request.IsMale,
                DateOfBirth = request.DateOfBirth,
                CityGeoNameId = request.CityGeoNameId
            };

            db.Patients.Add(patient);

            // Add phone numbers
            foreach (var phone in phoneNumbers)
            {
                db.PatientPhones.Add(new PatientPhone
                {
                    PatientId = patient.Id,
                    PhoneNumber = phone.PhoneNumber,
                    IsPrimary = phone.IsPrimary
                });
            }

            // Add chronic diseases
            foreach (var diseaseId in request.ChronicDiseaseIds)
            {
                db.PatientChronicDiseases.Add(new PatientChronicDisease
                {
                    PatientId = patient.Id,
                    ChronicDiseaseId = diseaseId
                });
            }

            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Patient created: {PatientId} {PatientCode} {PatientName} by {UserId} in {ClinicId}",
                patient.Id, patient.PatientCode, patient.FullName, currentUser.UserId, currentUser.ClinicId);

            // Load created patient with related data
            var createdPatient = await db.Patients
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

            return Results.Created($"/api/patients/{createdPatient.Id}", createdPatient);
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
        
        int? CityGeoNameId,
        
        [Required]
        [MinLength(1)]
        List<PhoneNumberInput> PhoneNumbers,
        
        [Required]
        List<Guid> ChronicDiseaseIds);

    public record PhoneNumberInput(
        [Required]
        [MaxLength(15)]
        string PhoneNumber,
        
        bool IsPrimary);

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
