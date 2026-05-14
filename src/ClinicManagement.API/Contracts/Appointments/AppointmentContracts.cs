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
    bool? MarkAsPaid = null,
    int? VisitDurationMinutes = null
);

public record UpdateAppointmentRequest(
    Guid VisitTypeId,
    string? ScheduledTime,
    decimal? DiscountPercent,
    int? VisitDurationMinutes = null
);

public record BulkCancelRequest(Guid DoctorInfoId, Guid BranchId, string Date);
public record RescheduleAppointmentRequest(string NewDate, Guid? NewBranchId = null);

public record UpdateStatusRequest(string Status);
public record SetAppointmentTypeRequest(string AppointmentType, Guid BranchId);
public record CheckInRequest(Guid DoctorInfoId, Guid BranchId);
public record HandleDelayRequest(string Option); // "AutoShift" | "MarkMissed" | "Manual" | "Cancel"
