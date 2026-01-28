using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientPhoneNumber : BaseEntity
{
    public int PatientId { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    
    public virtual Patient Patient { get; set; } = null!;
}