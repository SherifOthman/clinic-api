namespace ClinicManagement.API.Contracts.Clinic;

/// <summary>Request body for PATCH /api/clinic/settings</summary>
public record UpdateClinicSettingsRequest(int WeekStartDay);
