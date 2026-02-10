namespace ClinicManagement.Domain.Common.Enums;

/// <summary>
/// Defines the shift type for staff members
/// </summary>
public enum ShiftType
{
    /// <summary>
    /// Day shift (typically 8 AM - 4 PM)
    /// </summary>
    Day = 1,
    
    /// <summary>
    /// Night shift (typically 8 PM - 4 AM)
    /// </summary>
    Night = 2,
    
    /// <summary>
    /// Rotating shifts
    /// </summary>
    Rotating = 3,
    
    /// <summary>
    /// Flexible hours
    /// </summary>
    Flexible = 4
}
