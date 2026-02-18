namespace ClinicManagement.Application.Features.Auth.Queries.CheckEmailAvailability;

public record CheckEmailAvailabilityDto(
    bool IsAvailable,
    string? Message
);
