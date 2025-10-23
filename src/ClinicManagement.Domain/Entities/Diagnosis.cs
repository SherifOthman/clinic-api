using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Diagnosis : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
