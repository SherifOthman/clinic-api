namespace ClinicManagement.Domain.Common.Enums;

/// <summary>
/// Defines the type of user in the clinic system
/// </summary>
public enum UserType
{
    /// <summary>
    /// Clinic owner with full administrative access
    /// </summary>
    ClinicOwner = 1,
    
    /// <summary>
    /// Medical doctor providing consultations and treatments
    /// </summary>
    Doctor = 2,
    
    /// <summary>
    /// Receptionist handling appointments and front desk operations
    /// </summary>
    Receptionist = 3,
    
    /// <summary>
    /// Nurse providing patient care and assistance
    /// </summary>
    Nurse = 4,
    
    /// <summary>
    /// Pharmacist managing medicine inventory and dispensing
    /// </summary>
    Pharmacist = 5,
    
    /// <summary>
    /// Lab technician handling laboratory tests and results
    /// </summary>
    LabTechnician = 6,
    
    /// <summary>
    /// Accountant managing financial operations
    /// </summary>
    Accountant = 7
}
