using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class MedicineDispensing : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid MedicineId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime DispensedAt { get; set; }
    public Guid DispensedByUserId { get; set; }
    public DispensingStatus Status { get; set; }
    public string? Notes { get; set; }
    

    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Medicine Medicine { get; set; } = null!;
    

    public decimal TotalPrice => Quantity * UnitPrice;
}

public enum DispensingStatus
{
    Dispensed,
    PartiallyDispensed,
    Cancelled
}
