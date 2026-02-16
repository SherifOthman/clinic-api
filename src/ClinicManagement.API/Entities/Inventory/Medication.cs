using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class Medication : BaseEntity
{
    public string Name { get; set; } = null!;
}
