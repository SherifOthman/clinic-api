using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientPhone : AuditableEntity
{
    public Guid PatientId { get; set; }
    public string PhoneNumber { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
}
