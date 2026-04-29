using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

/// <summary>Get appointments for one or more doctors on a given date.</summary>
public record GetAppointmentsQuery(
    DateOnly Date,
    Guid? BranchId = null,
    List<Guid>? DoctorInfoIds = null
) : IRequest<List<AppointmentDto>>;

public record AppointmentDto(
    Guid Id,
    Guid DoctorInfoId,
    string DoctorName,
    Guid PatientId,
    string PatientName,
    string? PatientCode,
    int? QueueNumber,
    string? ScheduledTime,      // "HH:mm"
    string? EndTime,            // "HH:mm"
    int? VisitDurationMinutes,
    string Type,                // "Queue" | "Time"
    string Status,
    string VisitTypeNameEn,
    string VisitTypeNameAr,
    decimal FinalPrice,
    DateTimeOffset CreatedAt
);
