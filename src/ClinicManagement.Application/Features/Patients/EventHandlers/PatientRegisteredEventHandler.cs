using ClinicManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Patients.EventHandlers;

/// <summary>
/// Handles the PatientRegisteredEvent
/// This is where side effects happen AFTER the patient is successfully saved
/// Examples: sending welcome SMS, creating initial medical file, logging analytics
/// </summary>
public class PatientRegisteredEventHandler : INotificationHandler<PatientRegisteredEvent>
{
    private readonly ILogger<PatientRegisteredEventHandler> _logger;

    public PatientRegisteredEventHandler(ILogger<PatientRegisteredEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PatientRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Patient registered: {PatientId} - {PatientCode} - {FullName} at {OccurredOn}",
            notification.PatientId,
            notification.PatientCode,
            notification.FullName,
            notification.OccurredOn);

        // TODO: Add side effects here:
        // - Send welcome SMS to patient
        // - Create initial medical file
        // - Log analytics event
        // - Notify relevant staff members
        // - Send notification to clinic dashboard

        await Task.CompletedTask;
    }
}
