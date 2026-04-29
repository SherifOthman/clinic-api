namespace ClinicManagement.API.Contracts.Appointments;

public record CreateAppointmentRequest(
    Guid BranchId,
    Guid PatientId,
    Guid DoctorInfoId,
    Guid VisitTypeId,
    string Date,           // "YYYY-MM-DD"
    string Type,           // "Queue" | "Time"
    string? ScheduledTime, // "HH:mm" — required for Time
    decimal? DiscountPercent
);

public record UpdateStatusRequest(string Status);

public record SetAppointmentTypeRequest(string AppointmentType); // "Queue" | "Time"
