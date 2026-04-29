using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

/// <summary>Get appointments for one or more doctors on a given date.</summary>
public record GetAppointmentsQuery(
    DateOnly Date,
    List<Guid>? DoctorInfoIds = null  // null = all doctors in branch
) : IRequest<List<AppointmentDto>>;

public record AppointmentDto(
    Guid Id,
    Guid DoctorInfoId,
    string DoctorName,
    Guid PatientId,
    string PatientName,
    string? PatientCode,
    int? QueueNumber,
    string? ScheduledTime,   // "HH:mm"
    string Type,             // "Queue" | "Time"
    string Status,           // "Pending" | "InProgress" | "Completed" | "Cancelled" | "NoShow"
    string VisitTypeName,
    decimal FinalPrice,
    DateTimeOffset CreatedAt
);
