namespace ClinicManagement.Application.DTOs;

public class DoctorDto
{
    public Guid Id { get; set; }
    public Guid SpecializationId { get; set; }
    public short? YearsOfExperience { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public SpecializationDto? Specialization { get; set; }
}
