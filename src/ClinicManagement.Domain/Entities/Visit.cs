using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Visit : AuditableEntity
{
    public int AppointmentId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual Appointment Appointment { get; set; } = null!;
    public virtual ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; } = new List<PrescriptionMedicine>();
    public virtual ICollection<VisitAttributeValue> VisitAttributeValues { get; set; } = new List<VisitAttributeValue>();
}
