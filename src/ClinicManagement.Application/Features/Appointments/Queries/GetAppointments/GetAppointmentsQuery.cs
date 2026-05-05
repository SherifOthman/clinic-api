using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries;

/// <summary>Get appointments for one or more doctors on a given date.</summary>
public record GetAppointmentsQuery(
    DateOnly Date,
    Guid? BranchId = null,
    List<Guid>? DoctorInfoIds = null
) : IRequest<Result<List<AppointmentDto>>>;

public record AppointmentDto(
    Guid Id,
    Guid DoctorInfoId,
    string DoctorName,
    Guid PatientId,
    string PatientName,
    string? PatientCode,
    int? QueueNumber,
    string? ScheduledTime,
    string? EndTime,
    int? VisitDurationMinutes,
    string Type,
    string Status,
    string VisitTypeName,
    decimal FinalPrice,
    DateTimeOffset CreatedAt,
    string? PatientGender = null,
    DateOnly? PatientDateOfBirth = null
);
