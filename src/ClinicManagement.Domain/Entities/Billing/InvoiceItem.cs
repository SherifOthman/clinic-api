using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Invoice line item - supports multiple billable item types
/// Uses nullable FKs pattern: exactly ONE of the item type IDs should be set
/// This allows invoicing for services, medicines, supplies, lab tests, and radiology
/// </summary>
public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    
    // Flexible item types (exactly ONE should be set - validated in Invoice.AddItem)
    public Guid? MedicalServiceId { get; set; }
    public Guid? MedicineId { get; set; }
    public Guid? MedicalSupplyId { get; set; }
    public Guid? MedicineDispensingId { get; set; }  // Link to actual dispensing record
    public Guid? LabTestOrderId { get; set; }  // Link to lab test order
    public Guid? RadiologyOrderId { get; set; }  // Link to radiology order
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // SaleUnit only relevant for medicines (Box or Strip)
    // Medical services, supplies, lab tests, and radiology don't use this
    public SaleUnit? SaleUnit { get; set; }
    
    // Calculated property for total line amount
    public decimal LineTotal => Quantity * UnitPrice;


    public Invoice Invoice { get; set; } = null!;
    public MedicalService? MedicalService { get; set; }
    public Medicine? Medicine { get; set; }
    public MedicalSupply? MedicalSupply { get; set; }
    public MedicineDispensing? MedicineDispensing { get; set; }
    public LabTestOrder? LabTestOrder { get; set; }
    public RadiologyOrder? RadiologyOrder { get; set; }
}
