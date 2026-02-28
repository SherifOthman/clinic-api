namespace ClinicManagement.Domain.Entities;

public class DoctorProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StaffId { get; init; }
    public Guid? SpecializationId { get; set; }
    public int? YearsOfExperience { get; set; }
    
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Staff Staff { get; set; } = null!;
    public Specialization? Specialization { get; set; }
    public ICollection<DoctorSpecialization> DoctorSpecializations { get; set; } = new List<DoctorSpecialization>();
    public ICollection<DoctorWorkingDay> WorkingDays { get; set; } = new List<DoctorWorkingDay>();
}
