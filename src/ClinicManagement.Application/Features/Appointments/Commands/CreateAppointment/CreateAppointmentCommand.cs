using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<Result<AppointmentDto>>
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
