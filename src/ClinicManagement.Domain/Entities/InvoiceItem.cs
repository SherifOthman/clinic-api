using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    
    // Either service, medicine, or medical supply (only one should be set)
    public Guid? MedicalServiceId { get; set; }
    public Guid? MedicineId { get; set; }
    public Guid? MedicalSupplyId { get; set; }
    
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
}
