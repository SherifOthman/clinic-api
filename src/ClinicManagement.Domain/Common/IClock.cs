namespace ClinicManagement.Domain.Common;

/// <summary>
/// Abstraction for getting current time.
/// Allows for testable time-dependent code and consistent time handling.
/// </summary>
public interface IClock
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
    DateOnly Today { get; }
}
