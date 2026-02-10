using ClinicManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Appointments.EventHandlers;

/// <summary>
/// Handles AppointmentCreatedEvent - sends notifications and performs side effects
/// </summary>
public class AppointmentCreatedEventHandler : INotificationHandler<AppointmentCreatedEvent>
{
    private readonly ILogger<AppointmentCreatedEventHandler> _logger;
    // TODO: Inject IEmailService, ISmsService, etc. when implemented

    public AppointmentCreatedEventHandler(ILogger<AppointmentCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Appointment created: {AppointmentNumber} for patient {PatientId} with doctor {DoctorId} on {AppointmentDate}",
            notification.AppointmentNumber,
            notification.PatientId,
            notification.DoctorId,
            notification.AppointmentDate
        );

        // TODO: Implement side effects
        // 1. Send confirmation email to patient
        // await _emailService.SendAppointmentConfirmationAsync(notification.PatientId, notification.AppointmentNumber, ...);
        
        // 2. Send SMS reminder to patient
        // await _smsService.SendAppointmentReminderAsync(notification.PatientId, notification.AppointmentDate, ...);
        
        // 3. Notify doctor about new appointment
        // await _notificationService.NotifyDoctorAsync(notification.DoctorId, notification.AppointmentNumber, ...);
        
        // 4. Update clinic dashboard statistics
        // await _statisticsService.IncrementAppointmentCountAsync(notification.ClinicBranchId, ...);
        
        // 5. Log to audit trail
        // await _auditService.LogAppointmentCreatedAsync(notification, ...);

        await Task.CompletedTask;
    }
}
