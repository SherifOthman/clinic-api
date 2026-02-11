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
            .RequireAuthorization()
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
        CancellationToken ct)
    {
        var clinicId = currentUser.ClinicId!.Value;

        // Generate unique patient code
        var patientCode = await codeGenerator.GeneratePatientNumberAsync(clinicId, ct);

        // Ensure at least one primary phone
        var phoneNumbers = request.PhoneNumbers.ToList();
        if (!phoneNumbers.Any(p => p.IsPrimary) && phoneNumbers.Any())
        {
            phoneNumbers[0] = phoneNumbers[0] with { IsPrimary = true };
        }

        try
        {
            // Use domain factory method
            var patient = Patient.Create(
                patientCode,
                clinicId,
                request.FullName,
                request.Gender,
                request.DateOfBirth,
                request.CityGeoNameId);

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

            // Load created patient with related data
            var createdPatient = await db.Patients
                .Where(p => p.Id == patient.Id)
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
        [MaxLength(200, ErrorMessage = "Full name must not exceed 200 characters")]
        string FullName,
        
        [Required]
        [EnumDataType(typeof(Gender), ErrorMessage = "Invalid gender value")]
        Gender Gender,
        
        [Required]
        [CustomValidation(typeof(CustomValidators), nameof(CustomValidators.MustBeInPast))]
        DateTime DateOfBirth,
        
        int? CityGeoNameId,
        
        [Required]
        [MinLength(1, ErrorMessage = "At least one phone number is required")]
        List<PhoneNumberInput> PhoneNumbers,
        
        [Required]
        List<Guid> ChronicDiseaseIds);

    public record PhoneNumberInput(
        [Required]
        [MaxLength(20, ErrorMessage = "Phone number must not exceed 20 characters")]
        string PhoneNumber,
        
        bool IsPrimary);

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
