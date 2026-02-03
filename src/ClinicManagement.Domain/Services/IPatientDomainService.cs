using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Services;

/// <summary>
/// Domain service for Patient business logic
/// </summary>
public interface IPatientDomainService
{
    /// <summary>
    /// Calculates patient's age based on date of birth
    /// </summary>
    int CalculateAge(DateTime? dateOfBirth);
    
    /// <summary>
    /// Calculates patient's age at a specific date
    /// </summary>
    int CalculateAgeAt(DateTime? dateOfBirth, DateTime atDate);
    
    /// <summary>
    /// Determines if patient is a minor (under 18)
    /// </summary>
    bool IsMinor(DateTime? dateOfBirth);
    
    /// <summary>
    /// Determines if patient is elderly (65 or older)
    /// </summary>
    bool IsElderly(DateTime? dateOfBirth);
}