using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class AppointmentDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? ReceptionistId { get; set; }
    public AppointmentStatus Status { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public PatientDto? Patient { get; set; }
    public DoctorDto? Doctor { get; set; }
    public ReceptionistDto? Receptionist { get; set; }
}

public class CreateAppointmentDto
{
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int? ReceptionistId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateAppointmentDto
{
    public AppointmentStatus? Status { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public string? Notes { get; set; }
}

public class ReceptionistDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ClinicId { get; set; }
    public UserDto? User { get; set; }
    public ClinicDto? Clinic { get; set; }
}
