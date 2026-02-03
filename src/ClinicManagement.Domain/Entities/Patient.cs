using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Patient : BaseEntity
{
    public Guid UserId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public List<ClinicPatient> Clinics { get; set; } = new();


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
