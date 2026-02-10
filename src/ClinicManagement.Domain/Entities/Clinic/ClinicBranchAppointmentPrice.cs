using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Pricing configuration for appointment types per clinic branch
/// </summary>
public class ClinicBranchAppointmentPrice : AuditableEntity
{
    public Guid ClinicBranchId { get; set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public Guid AppointmentTypeId { get; set; }
    public AppointmentType AppointmentType { get; set; } = null!;
    
    /// <summary>
    /// Base price for this appointment type at this clinic branch
    /// </summary>
    public decimal Price { get; set; }
    
    /// <summary>
    /// Whether this appointment type is available at this clinic branch
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Optional description or notes about this pricing
    /// </summary>
    public string? Description { get; set; }
}
