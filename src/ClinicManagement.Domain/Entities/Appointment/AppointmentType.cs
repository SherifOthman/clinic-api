using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Types of appointments that can be booked
/// </summary>
public class AppointmentType : BaseEntity
{
    /// <summary>
    /// Name in English
    /// </summary>
    public string NameEn { get; set; } = string.Empty;
    
    /// <summary>
    /// Name in Arabic
    /// </summary>
    public string NameAr { get; set; } = string.Empty;
    
    /// <summary>
    /// Description in English
    /// </summary>
    public string? DescriptionEn { get; set; }
    
    /// <summary>
    /// Description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }
    
    /// <summary>
    /// Whether this appointment type is active and can be used
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Color code for UI display (optional)
    /// </summary>
    public string? ColorCode { get; set; }
    
    // Navigation properties
    public ICollection<ClinicBranchAppointmentPrice> ClinicBranchPrices { get; set; } = new List<ClinicBranchAppointmentPrice>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
