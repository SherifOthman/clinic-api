using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class DoctorProfile : AuditableEntity
{
    public Guid StaffId { get; init; }
    public Guid? SpecializationId { get; set; }

    // Navigation properties
    public Staff Staff { get; set; } = null!;
    public Specialization? Specialization { get; set; }
}
