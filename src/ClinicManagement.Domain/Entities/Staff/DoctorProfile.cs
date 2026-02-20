namespace ClinicManagement.Domain.Entities;

public class DoctorProfile
{
    public int Id { get; set; }
    public int StaffId { get; set; }
    public int? SpecializationId { get; set; }
    public int? YearsOfExperience { get; set; }
}
