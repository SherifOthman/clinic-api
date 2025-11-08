using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity
{
    public int BranchId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int Order { get; set; }
    public int? ReceptionistId { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public AppointmentType Type { get; set; }
    public DateTime AppointmentDate { get; set; }
    public decimal Price { get; set; }
    public decimal PaidPrice { get; set; }
    public decimal Discount { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual ClinicBranch Branch { get; set; } = null!;
    public virtual Patient Patient { get; set; } = null!;
    public virtual Doctor Doctor { get; set; } = null!;
    public virtual Receptionist? Receptionist { get; set; }
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
    
    // Domain methods
    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled appointments can be confirmed");
            
        Status = AppointmentStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Start()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed appointments can be started");
            
        Status = AppointmentStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Complete()
    {
        if (Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException("Only in-progress appointments can be completed");
            
        Status = AppointmentStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Cancel(string reason = "No reason provided")
    {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed appointment");
            
        Status = AppointmentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void MarkAsNoShow()
    {
        if (Status != AppointmentStatus.Confirmed && Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException("Only confirmed or scheduled appointments can be marked as no-show");
            
        Status = AppointmentStatus.NoShow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Reschedule(DateTime newDate)
    {
        if (Status == AppointmentStatus.Completed)
            throw new InvalidOperationException("Cannot reschedule a completed appointment");
            
        AppointmentDate = newDate;
        Status = AppointmentStatus.Rescheduled;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public decimal GetRemainingAmount()
    {
        return Price - PaidPrice - Discount;
    }
    
    public bool IsFullyPaid()
    {
        return GetRemainingAmount() <= 0;
    }
    
    public void AddPayment(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive");
            
        if (amount > GetRemainingAmount())
            throw new ArgumentException("Payment amount exceeds remaining balance");
            
        PaidPrice += amount;
        UpdatedAt = DateTime.UtcNow;
    }
}
