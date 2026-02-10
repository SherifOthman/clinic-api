using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class PatientDto
{
    public Guid Id { get; set; }
    public string PatientCode { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
    public int? CityGeoNameId { get; set; }
    public List<PatientPhoneDto> PhoneNumbers { get; set; } = new();
    public List<PatientChronicDiseaseDto> ChronicDiseases { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PatientPhoneDto
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

public class CreatePatientDto
{
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int? CityGeoNameId { get; set; }
    public List<CreatePatientPhoneDto> PhoneNumbers { get; set; } = new();
    public List<Guid> ChronicDiseaseIds { get; set; } = new();
}

public class CreatePatientPhoneDto
{
    public string PhoneNumber { get; set; } = null!;
    public bool IsPrimary { get; set; }
}

public class UpdatePatientDto
{
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int? CityGeoNameId { get; set; }
    public List<UpdatePatientPhoneDto> PhoneNumbers { get; set; } = new();
    public List<Guid> ChronicDiseaseIds { get; set; } = new();
}

public class UpdatePatientPhoneDto
{
    public Guid? Id { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public bool IsPrimary { get; set; }
}
