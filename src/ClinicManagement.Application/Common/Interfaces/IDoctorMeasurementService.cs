namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Service for managing doctor measurements
/// Handles copying specialty defaults to doctor's working set
/// </summary>
public interface IDoctorMeasurementService
{
    /// <summary>
    /// Copies measurement defaults from specialty to doctor when doctor is created
    /// </summary>
    /// <param name="doctorId">The doctor's user ID</param>
    /// <param name="specialtyId">The doctor's specialty ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task CopySpecialtyMeasurementDefaultsAsync(Guid doctorId, Guid specialtyId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets enabled measurements for a doctor
    /// </summary>
    /// <param name="doctorId">The doctor's user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of enabled doctor measurements</returns>
    Task<List<Domain.Entities.DoctorMeasurement>> GetDoctorMeasurementsAsync(Guid doctorId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates doctor measurement settings
    /// </summary>
    /// <param name="doctorId">The doctor's user ID</param>
    /// <param name="measurementId">The measurement definition ID</param>
    /// <param name="isEnabled">Whether the measurement is enabled</param>
    /// <param name="isRequired">Whether the measurement is required</param>
    /// <param name="displayOrder">Display order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task UpdateDoctorMeasurementAsync(Guid doctorId, Guid measurementId, bool isEnabled, bool isRequired, int displayOrder, CancellationToken cancellationToken = default);
}