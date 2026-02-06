namespace ClinicManagement.Domain.Common.Enums;

/// <summary>
/// Defines the type of user in the clinic system
/// </summary>
public enum UserType
{
    /// <summary>
    /// Super admin with system-wide access across all clinics
    /// </summary>
    SuperAdmin = 0,
    
    /// <summary>
    /// Clinic owner with full administrative access to their clinic
    /// </summary>
    ClinicOwner = 1,
    
    /// <summary>
    /// Medical doctor providing consultations and treatments
    /// </summary>
    Doctor = 2,
    
    /// <summary>
    /// Receptionist handling appointments and front desk operations
    /// </summary>
    Receptionist = 3
}
