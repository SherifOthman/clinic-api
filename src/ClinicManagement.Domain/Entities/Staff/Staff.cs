using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Staff : TenantEntity
{
    public int UserId { get; set; }
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; } = true;
}
