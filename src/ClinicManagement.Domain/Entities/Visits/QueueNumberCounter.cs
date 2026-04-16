using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class QueueNumberCounter : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Guid ClinicBranchId { get; set; }
    public DateOnly Date { get; set; }
    public int Value { get; set; }
}

