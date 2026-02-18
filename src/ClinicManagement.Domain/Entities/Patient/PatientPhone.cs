using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientPhone : BaseEntity
{
    public Guid PatientId { get; set; }
    
    public bool IsPrimary { get; set; } = false;
}
