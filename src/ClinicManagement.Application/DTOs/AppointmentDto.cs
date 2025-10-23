using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? ReceptionistId { get; set; }
    public AppointmentStatus Status { get; set; }
    public AppointmentType Type { get; set; }
    public DateTime AppointmentDate { get; set; }
    public decimal Price { get; set; }
    public decimal PaidPrice { get; set; }
    public decimal Discount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ClinicBranchDto? Branch { get; set; }
    public PatientDto? Patient { get; set; }
    public DoctorDto? Doctor { get; set; }
    public ReceptionistDto? Receptionist { get; set; }
    public List<VisitDto> Visits { get; set; } = new();
}

public class CreateAppointmentDto
{
    public int BranchId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? ReceptionistId { get; set; }
    public AppointmentType Type { get; set; }
    public DateTime AppointmentDate { get; set; }
    public decimal Price { get; set; }
    public decimal? PaidPrice { get; set; }
    public decimal? Discount { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAppointmentDto
{
    public AppointmentStatus? Status { get; set; }
    public AppointmentType? Type { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public decimal? Price { get; set; }
    public decimal? PaidPrice { get; set; }
    public decimal? Discount { get; set; }
    public string? Notes { get; set; }
}

public class ClinicBranchDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public int? CityId { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public ClinicDto? Clinic { get; set; }
    public CityDto? CityNavigation { get; set; }
}

public class CreateClinicBranchDto
{
    public int ClinicId { get; set; }
    public int? CityId { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
}

public class ReceptionistDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BranchId { get; set; }
    public UserDto? User { get; set; }
    public ClinicBranchDto? Branch { get; set; }
}
