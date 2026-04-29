namespace ClinicManagement.API.Contracts.Appointments;

public record CreateAppointmentRequest(
    Guid BranchId,
    Guid PatientId,
    Guid DoctorInfoId,
    Guid VisitTypeId,
    string Date,
    string Type,
    string? ScheduledTime,
    decimal? DiscountPercent,
    int? VisitDurationMinutes = null
);

public record UpdateStatusRequest(string Status);
public record SetAppointmentTypeRequest(string AppointmentType);
public record CheckInRequest(Guid DoctorInfoId, Guid BranchId);
public record HandleDelayRequest(string Option); // "AutoShift" | "MarkMissed" | "Manual"
