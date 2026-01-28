using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand : IRequest<Result<PatientDto>>
{
    public string FullName { get; init; } = string.Empty;
    public DateTime? DateOfBirth { get; init; }
    public Gender? Gender { get; init; }
    
    // Location fields
    public string? Address { get; init; }
    public int? GeoNameId { get; init; }
    
    // Phone numbers
    public List<CreatePatientPhoneNumberDto> PhoneNumbers { get; init; } = new();
    
    // Chronic diseases
    public List<int> ChronicDiseaseIds { get; init; } = new();
}
