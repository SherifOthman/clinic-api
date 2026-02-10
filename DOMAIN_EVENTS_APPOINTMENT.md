# Domain Events for Appointment Aggregate - Complete ✅

## Overview

Added comprehensive domain events to the Appointment aggregate to enable loose coupling and side effects when important business events occur.

## Domain Events Created

### 1. AppointmentCreatedEvent

**File**: `src/ClinicManagement.Domain/Events/AppointmentCreatedEvent.cs`

**Raised When**: A new appointment is created via `Appointment.Create()`

**Data**:

- AppointmentId
- ClinicBranchId
- PatientId
- DoctorId
- AppointmentNumber
- AppointmentDate
- QueueNumber
- FinalPrice

**Potential Use Cases**:

- Send confirmation SMS/email to patient
- Send notification to doctor
- Update clinic dashboard statistics
- Log appointment creation for audit
- Trigger external calendar integration

### 2. AppointmentConfirmedEvent

**File**: `src/ClinicManagement.Domain/Events/AppointmentConfirmedEvent.cs`

**Raised When**: Appointment status changes from Pending to Confirmed via `appointment.Confirm()`

**Data**:

- AppointmentId
- PatientId
- DoctorId
- AppointmentDate
- QueueNumber

**Potential Use Cases**:

- Send confirmation notification to patient
- Update doctor's schedule
- Reserve resources (room, equipment)
- Update waiting list if applicable

### 3. AppointmentCompletedEvent

**File**: `src/ClinicManagement.Domain/Events/AppointmentCompletedEvent.cs`

**Raised When**: Appointment status changes from Confirmed to Completed via `appointment.Complete()`

**Data**:

- AppointmentId
- PatientId
- DoctorId
- AppointmentDate
- FinalPrice
- PaidAmount

**Potential Use Cases**:

- Generate invoice if not fully paid
- Update doctor performance metrics
- Request patient feedback/rating
- Update patient visit history
- Trigger follow-up appointment reminder

### 4. AppointmentCancelledEvent

**File**: `src/ClinicManagement.Domain/Events/AppointmentCancelledEvent.cs`

**Raised When**: Appointment is cancelled via `appointment.Cancel(reason)`

**Data**:

- AppointmentId
- PatientId
- DoctorId
- AppointmentDate
- Reason

**Potential Use Cases**:

- Send cancellation notification to patient and doctor
- Free up the time slot
- Update waiting list (offer slot to next patient)
- Track cancellation patterns for analytics
- Process refund if payment was made

### 5. AppointmentPaymentRecordedEvent

**File**: `src/ClinicManagement.Domain/Events/AppointmentPaymentRecordedEvent.cs`

**Raised When**: Payment is recorded via `appointment.RecordPayment(amount)`

**Data**:

- AppointmentId
- PatientId
- PaymentAmount
- RemainingAmount
- IsFullyPaid

**Potential Use Cases**:

- Send payment receipt to patient
- Update accounting system
- Send reminder for remaining balance if partially paid
- Update patient payment history
- Trigger loyalty points/rewards

## Implementation Details

### Appointment Entity Changes

**Before**: Inherited from `AuditableEntity`

```csharp
public class Appointment : AuditableEntity
{
    // No domain events
}
```

**After**: Inherits from `AggregateRoot`

```csharp
public class Appointment : AggregateRoot
{
    // Can raise domain events

    public static Appointment Create(...)
    {
        var appointment = new Appointment { ... };

        // Raise domain event
        appointment.AddDomainEvent(new AppointmentCreatedEvent(...));

        return appointment;
    }

    public void Confirm()
    {
        Status = AppointmentStatus.Confirmed;
        AddDomainEvent(new AppointmentConfirmedEvent(...));
    }

    // Similar for Complete(), Cancel(), RecordPayment()
}
```

### Event Dispatching

Events are automatically dispatched by `ApplicationDbContext.SaveChangesAsync()`:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    // ... other logic ...

    // Dispatch domain events
    await DispatchDomainEventsAsync(cancellationToken);

    return result;
}
```

This ensures:

- ✅ Events are only dispatched after successful save (consistency)
- ✅ Events are dispatched in the same transaction
- ✅ If save fails, events are not dispatched

## Benefits

### 1. Loose Coupling

- Appointment aggregate doesn't need to know about email service, notification service, etc.
- Side effects are handled by event handlers
- Easy to add new side effects without modifying aggregate

### 2. Single Responsibility

- Appointment focuses on business rules
- Event handlers focus on side effects
- Clear separation of concerns

### 3. Testability

- Can test appointment logic without mocking email/notification services
- Can test event handlers independently
- Domain events are part of the test assertions

### 4. Audit Trail

- Every important business event is captured
- Easy to implement event sourcing later if needed
- Clear history of what happened and when

### 5. Extensibility

- Easy to add new event handlers without changing existing code
- Multiple handlers can listen to the same event
- Handlers can be in different projects/assemblies

## Example Event Handler

Here's how you would create an event handler:

```csharp
// In Application layer
public class AppointmentCreatedEventHandler : INotificationHandler<AppointmentCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public AppointmentCreatedEventHandler(IEmailService emailService, ISmsService smsService)
    {
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task Handle(AppointmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Send confirmation email to patient
        await _emailService.SendAppointmentConfirmationAsync(
            notification.PatientId,
            notification.AppointmentNumber,
            notification.AppointmentDate,
            cancellationToken
        );

        // Send SMS reminder
        await _smsService.SendAppointmentReminderAsync(
            notification.PatientId,
            notification.AppointmentDate,
            notification.QueueNumber,
            cancellationToken
        );
    }
}
```

## Test Results

```
Total Tests: 210
- Application Tests: 6
- Domain Tests: 204
  - Email: 21
  - Money: 43
  - Patient: 51
  - Invoice: 29
  - Appointment: 60

Status: ✅ All 210 tests passing
Build: ✅ Success
```

## Files Created

### Domain Events

- ✅ `src/ClinicManagement.Domain/Events/AppointmentCreatedEvent.cs`
- ✅ `src/ClinicManagement.Domain/Events/AppointmentConfirmedEvent.cs`
- ✅ `src/ClinicManagement.Domain/Events/AppointmentCompletedEvent.cs`
- ✅ `src/ClinicManagement.Domain/Events/AppointmentCancelledEvent.cs`
- ✅ `src/ClinicManagement.Domain/Events/AppointmentPaymentRecordedEvent.cs`

### Modified Files

- ✅ `src/ClinicManagement.Domain/Entities/Appointment/Appointment.cs`
  - Changed base class from `AuditableEntity` to `AggregateRoot`
  - Added domain event raising in `Create()`, `Confirm()`, `Complete()`, `Cancel()`, `RecordPayment()`

## Next Steps

### 1. Create Event Handlers (Application Layer)

Create handlers for each event to implement side effects:

- `AppointmentCreatedEventHandler` - Send notifications
- `AppointmentConfirmedEventHandler` - Update schedules
- `AppointmentCompletedEventHandler` - Generate invoices, request feedback
- `AppointmentCancelledEventHandler` - Process cancellations, update waiting list
- `AppointmentPaymentRecordedEventHandler` - Send receipts, update accounting

### 2. Add Domain Events to Other Aggregates

- **Invoice**: InvoiceIssuedEvent, InvoiceFullyPaidEvent, InvoiceCancelledEvent
- **Patient**: PatientPhoneNumberAddedEvent, PatientChronicDiseaseAddedEvent
- **Medicine**: MedicineStockLowEvent, MedicineExpiredEvent

### 3. Implement Event Sourcing (Optional)

- Store all domain events in an event store
- Rebuild aggregate state from events
- Enable time travel debugging
- Support CQRS pattern

### 4. Add Integration Events (Optional)

For cross-bounded-context communication:

- `AppointmentCreatedIntegrationEvent` - Notify other microservices
- Use message bus (RabbitMQ, Azure Service Bus, etc.)

## Architecture Compliance

### ✅ DDD Patterns Applied

1. **Domain Events** - Capture important business events
2. **Aggregate Root** - Appointment inherits from AggregateRoot
3. **Event Dispatching** - Automatic dispatch after SaveChanges
4. **Loose Coupling** - Side effects handled by event handlers

### ✅ Clean Architecture

1. **Domain Layer** - Pure domain events, no dependencies
2. **Application Layer** - Event handlers with side effects
3. **Infrastructure Layer** - Event dispatching mechanism

### ✅ Industry Standards

Following patterns from:

- Eric Evans (Domain-Driven Design)
- Vaughn Vernon (Implementing DDD)
- Udi Dahan (Domain Events pattern)
- Microsoft eShopOnContainers

## Summary

✅ **Created 5 domain events** for Appointment aggregate
✅ **Updated Appointment entity** to raise events
✅ **All tests passing** (210/210)
✅ **No breaking changes**
✅ **Ready for event handlers** to be implemented

The Appointment aggregate now properly communicates important business events through domain events, enabling loose coupling and extensibility!
