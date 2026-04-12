namespace ClinicManagement.Application.Common.Models;

/// <summary>
/// Shared response for email/username availability checks.
/// </summary>
public record AvailabilityDto(bool IsAvailable, string? Message);
