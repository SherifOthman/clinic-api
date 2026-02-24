using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Specialization : BaseEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
