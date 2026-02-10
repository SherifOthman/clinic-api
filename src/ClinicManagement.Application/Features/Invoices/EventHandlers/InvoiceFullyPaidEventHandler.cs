using ClinicManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Invoices.EventHandlers;

/// <summary>
/// Handles InvoiceFullyPaidEvent - sends receipt and performs accounting updates
/// </summary>
public class InvoiceFullyPaidEventHandler : INotificationHandler<InvoiceFullyPaidEvent>
{
    private readonly ILogger<InvoiceFullyPaidEventHandler> _logger;
    // TODO: Inject IEmailService, IAccountingService, etc. when implemented

    public InvoiceFullyPaidEventHandler(ILogger<InvoiceFullyPaidEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(InvoiceFullyPaidEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Invoice fully paid: {InvoiceNumber} for patient {PatientId}, Total: {TotalAmount}",
            notification.InvoiceNumber,
            notification.PatientId,
            notification.TotalAmount
        );

        // TODO: Implement side effects
        // 1. Send payment receipt to patient
        // await _emailService.SendPaymentReceiptAsync(notification.PatientId, notification.InvoiceNumber, ...);
        
        // 2. Update accounting system
        // await _accountingService.RecordFullPaymentAsync(notification.InvoiceId, notification.TotalAmount, ...);
        
        // 3. Update patient payment history
        // await _patientService.UpdatePaymentHistoryAsync(notification.PatientId, notification.TotalAmount, ...);
        
        // 4. Trigger loyalty points/rewards
        // await _loyaltyService.AwardPointsAsync(notification.PatientId, notification.TotalAmount, ...);
        
        // 5. Close any outstanding payment reminders
        // await _reminderService.CancelPaymentRemindersAsync(notification.InvoiceId, ...);

        await Task.CompletedTask;
    }
}
