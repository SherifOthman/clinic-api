using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public List<ClinicPatient> Clinics { get; set; } = new();

    // Business logic moved to Domain Service
    // Use IPatientDomainService.CalculateAge(this.DateOfBirth)
}
