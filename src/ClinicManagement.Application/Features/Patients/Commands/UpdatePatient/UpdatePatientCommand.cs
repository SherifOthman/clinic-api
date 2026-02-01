using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand : IRequest<Result<PatientDto>>
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public DateTime? DateOfBirth { get; init; }
    public Gender? Gender { get; init; }
    
    // Location fields
    public string? Address { get; init; }
    public int? GeoNameId { get; init; }
    
    // Phone numbers
    public List<UpdatePatientPhoneNumberDto> PhoneNumbers { get; init; } = new();
    
    // Chronic diseases
    public List<Guid> ChronicDiseaseIds { get; init; } = new();
}
