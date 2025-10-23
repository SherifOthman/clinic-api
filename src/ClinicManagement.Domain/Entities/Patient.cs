using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : BaseEntity
{
    public int ClinicId { get; set; }
    public string? Avatar { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string ThirdName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? GeneralNotes { get; set; }
    
    // Navigation properties
    public virtual Clinic Clinic { get; set; } = null!;
    public virtual ICollection<PatientSurgery> Surgeries { get; set; } = new List<PatientSurgery>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    
    // Domain methods
    public string GetFullName()
    {
        var parts = new List<string> { FirstName };
        if (!string.IsNullOrEmpty(SecondName))
            parts.Add(SecondName);
        parts.Add(ThirdName);
        return string.Join(" ", parts);
    }
    
    public int GetAge()
    {
        if (!DateOfBirth.HasValue)
            return 0;
            
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Value.Year;
        if (DateOfBirth.Value.Date > today.AddYears(-age))
            age--;
        return age;
    }
    
    public void AddSurgery(string name, string description)
    {
        Surgeries.Add(new PatientSurgery
        {
            PatientId = Id,
            Name = name,
            Description = description
        });
    }
}
