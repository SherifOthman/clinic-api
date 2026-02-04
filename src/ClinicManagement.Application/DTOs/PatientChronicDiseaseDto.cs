namespace ClinicManagement.Application.DTOs;

public class PatientChronicDiseaseDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid ChronicDiseaseId { get; set; }
    public DateTime? DiagnosedDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ChronicDiseaseDto? ChronicDisease { get; set; }
}

public class CreatePatientChronicDiseaseDto
{
    public Guid ChronicDiseaseId { get; set; }
    public DateTime? DiagnosedDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePatientChronicDiseaseDto
{
    public DateTime? DiagnosedDate { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}
