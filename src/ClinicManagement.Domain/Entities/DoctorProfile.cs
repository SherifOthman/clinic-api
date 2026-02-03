using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class DoctorProfile : AuditableEntity
{
    public string Specialty { get; set; } = null!;
    public string AdditionalInfo { get; set; } = null!; // Any extra info
}