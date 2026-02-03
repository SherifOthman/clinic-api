namespace ClinicManagement.Domain.ValueObjects;

/// <summary>
/// Value object representing a patient's age with additional metadata
/// </summary>
public record PatientAge
{
    public int Years { get; init; }
    public bool IsMinor { get; init; }
    public bool IsElderly { get; init; }
    public string AgeGroup { get; init; } = string.Empty;
    
    public static PatientAge Create(int years)
    {
        var isMinor = years < 18;
        var isElderly = years >= 65;
        
        var ageGroup = years switch
        {
            < 2 => "Infant",
            < 12 => "Child", 
            < 18 => "Adolescent",
            < 65 => "Adult",
            _ => "Elderly"
        };
        
        return new PatientAge
        {
            Years = years,
            IsMinor = isMinor,
            IsElderly = isElderly,
            AgeGroup = ageGroup
        };
    }
}