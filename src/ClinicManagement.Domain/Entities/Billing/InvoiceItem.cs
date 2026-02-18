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
    public int InvoiceId { get; set; }
    
    // Flexible item types (exactly ONE should be set - validated in Invoice.AddItem)
    public int? MedicalServiceId { get; set; }
    public int? MedicineId { get; set; }
    public int? MedicalSupplyId { get; set; }
    public int? MedicineDispensingId { get; set; }  // Link to actual dispensing record
    public int? LabTestOrderId { get; set; }  // Link to lab test order
    public int? RadiologyOrderId { get; set; }  // Link to radiology order
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // SaleUnit only relevant for medicines (Box or Strip)
    // Medical services, supplies, lab tests, and radiology don't use this
    public SaleUnit? SaleUnit { get; set; }
    

    public decimal LineTotal => Quantity * UnitPrice;
    public MedicalService? MedicalService { get; set; }
    public Medicine? Medicine { get; set; }
    public MedicalSupply? MedicalSupply { get; set; }
    public MedicineDispensing? MedicineDispensing { get; set; }
    public LabTestOrder? LabTestOrder { get; set; }
    public RadiologyOrder? RadiologyOrder { get; set; }
}
