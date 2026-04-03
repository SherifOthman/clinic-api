namespace ClinicManagement.Domain.Entities;

public class DoctorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StaffId { get; init; }
    public Guid? SpecializationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Specialization? Specialization { get; set; }
}
