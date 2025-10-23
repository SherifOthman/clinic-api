using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand : IRequest<Result<PatientDto>>
{
    public int Id { get; set; }
    public string? Avatar { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string ThirdName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? GeneralNotes { get; set; }
}
