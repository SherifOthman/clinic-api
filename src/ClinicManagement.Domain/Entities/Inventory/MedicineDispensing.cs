using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class MedicineDispensing : BaseEntity
{
    public int ClinicBranchId { get; set; }
    public int PatientId { get; set; }
    public int MedicineId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime DispensedAt { get; set; }
    public int DispensedByUserId { get; set; }
    public DispensingStatus Status { get; set; }
    public string? Notes { get; set; }
    

    public decimal TotalPrice => Quantity * UnitPrice;
}

public enum DispensingStatus
{
    Dispensed,
    PartiallyDispensed,
    Cancelled
}
