using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Specialization : BaseEntity
{
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; } = true;
}
