using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Services;

/// <summary>
/// Domain service implementing Patient business logic
/// </summary>
public class PatientDomainService : IPatientDomainService
{
    public int CalculateAge(DateTime? dateOfBirth)
    {
        return CalculateAgeAt(dateOfBirth, DateTime.Today);
    }
    
    public int CalculateAgeAt(DateTime? dateOfBirth, DateTime atDate)
    {
        if (!dateOfBirth.HasValue)
            return 0;
            
        var age = atDate.Year - dateOfBirth.Value.Year;
        
        // Adjust if birthday hasn't occurred yet this year
        if (dateOfBirth.Value.Date > atDate.AddYears(-age))
            age--;
            
        return Math.Max(0, age); // Ensure non-negative age
    }
    
    public bool IsMinor(DateTime? dateOfBirth)
    {
        return CalculateAge(dateOfBirth) < 18;
    }
    
    public bool IsElderly(DateTime? dateOfBirth)
    {
        return CalculateAge(dateOfBirth) >= 65;
    }
}