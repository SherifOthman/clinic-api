using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;

namespace ClinicManagement.API.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    
    // Flexible item types (only ONE should be set)
    public Guid? MedicalServiceId { get; set; }
    public Guid? MedicineId { get; set; }
    public Guid? MedicalSupplyId { get; set; }
    public Guid? MedicineDispensingId { get; set; }  // NEW: Link to actual dispensing
    public Guid? LabTestOrderId { get; set; }  // NEW: Link to lab test
    public Guid? RadiologyOrderId { get; set; }  // NEW: Link to radiology test
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Only relevant for medicines (Box or Strip)
    // Medical supplies and services don't use this
    public SaleUnit? SaleUnit { get; set; }
    
    // Calculated property for total line amount
    public decimal LineTotal => Quantity * UnitPrice;

    // Navigation properties
    public Invoice Invoice { get; set; } = null!;
    public MedicalService? MedicalService { get; set; }
    public Medicine? Medicine { get; set; }
    public MedicalSupply? MedicalSupply { get; set; }
    public MedicineDispensing? MedicineDispensing { get; set; }  // NEW
    public LabTestOrder? LabTestOrder { get; set; }  // NEW
    public RadiologyOrder? RadiologyOrder { get; set; }  // NEW
}
