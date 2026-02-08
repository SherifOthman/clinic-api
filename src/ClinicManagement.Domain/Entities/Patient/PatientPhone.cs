using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientPhone : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;
    
    public bool IsPrimary { get; set; } = false;
}
