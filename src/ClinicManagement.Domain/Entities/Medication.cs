using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Global medication reference data (no ClinicId).
/// Shared across all tenants for standardization.
/// System Admin manages this data.
/// </summary>
public class Medication : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Strength { get; set; } // e.g., "500mg", "10mg/ml"
    public string? Form { get; set; } // e.g., "Tablet", "Capsule", "Syrup"
    public string? ActiveIngredient { get; set; }
    public string? Manufacturer { get; set; }
    public string? Description { get; set; }
    public string? Contraindications { get; set; }
    public string? SideEffects { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<ClinicMedication> ClinicMedications { get; set; } = new List<ClinicMedication>();
}