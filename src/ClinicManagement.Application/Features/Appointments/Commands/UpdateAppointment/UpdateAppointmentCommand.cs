using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.UpdateAppointment;

public record UpdateAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public int Id { get; set; }
    public AppointmentStatus? Status { get; set; }
    public AppointmentType? Type { get; set; }
    public DateTime? AppointmentDate { get; set; }
    public decimal? Price { get; set; }
    public decimal? PaidPrice { get; set; }
    public decimal? Discount { get; set; }
    public string? Notes { get; set; }
}
