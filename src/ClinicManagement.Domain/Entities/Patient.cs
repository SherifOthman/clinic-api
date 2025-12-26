using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : AuditableEntity
{
    public int ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    
    // Navigation properties
    public virtual Clinic Clinic { get; set; } = null!;
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    
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
}
