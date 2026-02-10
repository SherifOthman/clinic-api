using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Medication : BaseEntity
{
    public string Name { get; set; } = null!;
}
