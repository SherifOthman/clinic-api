using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class SpecializationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateSpecializationDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class DoctorDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SpecializationId { get; set; }
    public string? Bio { get; set; }
    public bool IsActive { get; set; }
    public UserDto? User { get; set; }
    public SpecializationDto? Specialization { get; set; }
}

public class CreateDoctorDto
{
    public int UserId { get; set; }
    public int SpecializationId { get; set; }
    public string? Bio { get; set; }
}

public class PatientDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
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
    public ClinicDto? Clinic { get; set; }
    public List<PatientSurgeryDto> Surgeries { get; set; } = new();
}

public class CreatePatientDto
{
    public int ClinicId { get; set; }
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

public class UpdatePatientDto
{
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

public class PatientSurgeryDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
