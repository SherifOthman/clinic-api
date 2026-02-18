namespace ClinicManagement.Application.Features.Auth.Queries.CheckUsernameAvailability;

public record CheckUsernameAvailabilityDto(
    bool IsAvailable,
    string? Message
);
