namespace ClinicManagement.Domain.Entities;

public class DoctorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StaffId { get; init; }
    public Guid? SpecializationId { get; set; }
    public int? YearsOfExperience { get; set; }
    
    // License tracking (US-2)
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
