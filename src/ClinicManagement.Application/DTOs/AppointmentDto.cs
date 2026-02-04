using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class AppointmentDto
{
    public Guid Id { get; set; }
    public string AppointmentNumber { get; set; } = string.Empty;
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public short QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public string? PatientName { get; set; }
    public string? DoctorName { get; set; }
    public string? AppointmentTypeName { get; set; }
    public string? ClinicBranchName { get; set; }
    public AppointmentTypeDto? AppointmentType { get; set; }
}

public class CreateAppointmentDto
{
    public Guid ClinicBranchId { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public decimal? CustomPrice { get; set; } // If null, use default price from ClinicBranchAppointmentPrice
    public decimal DiscountAmount { get; set; } = 0;
    public decimal PaidAmount { get; set; } = 0;
}

public class UpdateAppointmentDto
{
    public DateTime AppointmentDate { get; set; }
    public AppointmentStatus Status { get; set; }
    public Guid AppointmentTypeId { get; set; }
    public decimal FinalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PaidAmount { get; set; }
}
