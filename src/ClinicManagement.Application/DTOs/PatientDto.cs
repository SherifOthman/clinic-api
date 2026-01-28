using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class PatientDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public int Age { get; set; }
    
    // Location fields
    public string? Address { get; set; }
    public int? GeoNameId { get; set; }
    
    // Phone numbers
    public List<PatientPhoneNumberDto> PhoneNumbers { get; set; } = new();
    
    // Chronic diseases
    public List<PatientChronicDiseaseDto> ChronicDiseases { get; set; } = new();
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PatientPhoneNumberDto
{
    public int Id { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
}

public class PatientChronicDiseaseDto
{
    public int Id { get; set; }
    public int ChronicDiseaseId { get; set; }
    public string ChronicDiseaseName { get; set; } = string.Empty;
    public DateTime DiagnosedDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreatePatientDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    
    // Location fields
    public string? Address { get; set; }
    public int? GeoNameId { get; set; }
    
    // Phone numbers
    public List<CreatePatientPhoneNumberDto> PhoneNumbers { get; set; } = new();
    
    // Chronic diseases
    public List<CreatePatientChronicDiseaseDto> ChronicDiseases { get; set; } = new();
}

public class CreatePatientPhoneNumberDto
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class CreatePatientChronicDiseaseDto
{
    public int ChronicDiseaseId { get; set; }
    public DateTime DiagnosedDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePatientDto
{
    public string FullName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    
    // Location fields
    public string? Address { get; set; }
    public int? GeoNameId { get; set; }
    
    // Phone numbers
    public List<UpdatePatientPhoneNumberDto> PhoneNumbers { get; set; } = new();
    
    // Chronic diseases
    public List<UpdatePatientChronicDiseaseDto> ChronicDiseases { get; set; } = new();
}

public class UpdatePatientPhoneNumberDto
{
    public int? Id { get; set; } // null for new phone numbers
    public string PhoneNumber { get; set; } = string.Empty;
}

public class UpdatePatientChronicDiseaseDto
{
    public int? Id { get; set; } // null for new chronic diseases
    public int ChronicDiseaseId { get; set; }
    public DateTime DiagnosedDate { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
