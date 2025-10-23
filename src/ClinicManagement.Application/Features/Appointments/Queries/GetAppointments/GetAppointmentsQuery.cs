using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public record GetAppointmentsQuery : IRequest<Result<PaginatedList<AppointmentDto>>>
{
    public int? BranchId { get; set; }
    public int? PatientId { get; set; }
    public int? DoctorId { get; set; }
    public AppointmentStatus? Status { get; set; }
    public AppointmentType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
