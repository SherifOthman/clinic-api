using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Common.Validation;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Appointments;

public class CreateAppointmentEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/appointments", HandleAsync)
            .RequireAuthorization()
            .WithName("CreateAppointment")
            .WithSummary("Create a new appointment")
            .WithTags("Appointments")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        DateTimeProvider dateTime,
        ILogger<CreateAppointmentEndpoint> logger,
        CancellationToken ct)
    {
        // Business Rule 1: Verify patient exists (global query filter ensures it belongs to clinic)
        var patientExists = await db.Patients
            .AnyAsync(p => p.Id == request.PatientId, ct);

        if (!patientExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.PATIENT_NOT_FOUND,
                Title = "Patient Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Patient not found or does not belong to your clinic"
            });

        // Business Rule 2: Verify doctor exists (global query filter ensures it belongs to clinic)
        var doctorExists = await db.DoctorProfiles
            .AnyAsync(d => d.Id == request.DoctorId, ct);

        if (!doctorExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.DOCTOR_NOT_FOUND,
                Title = "Doctor Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Doctor not found or does not belong to your clinic"
            });

        // Business Rule 3: Verify branch exists (global query filter ensures it belongs to clinic)
        var branchExists = await db.ClinicBranches
            .AnyAsync(b => b.Id == request.ClinicBranchId, ct);

        if (!branchExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.BRANCH_NOT_FOUND,
                Title = "Branch Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Branch not found or does not belong to your clinic"
            });

        // Business Rule 4: Check for appointment conflicts (same doctor, same date/time)
        var appointmentDate = request.ScheduledAt.Date;
        var hasConflict = await db.Appointments
            .AnyAsync(a =>
                a.DoctorId == request.DoctorId &&
                a.AppointmentDate == appointmentDate &&
                a.Status != AppointmentStatus.Cancelled, ct);

        if (hasConflict)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.APPOINTMENT_CONFLICT,
                Title = "Appointment Conflict",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Doctor already has an appointment on this date"
            });

        // Generate appointment number
        var appointmentNumber = await GenerateAppointmentNumberAsync(db, dateTime, request.ClinicBranchId, ct);

        // Get next queue number for the day
        var queueNumber = await GetNextQueueNumberAsync(db, request.ClinicBranchId, appointmentDate, ct);

        try
        {
            var appointment = new Appointment
            {
                AppointmentNumber = appointmentNumber,
                ClinicBranchId = request.ClinicBranchId,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                AppointmentTypeId = request.AppointmentTypeId,
                AppointmentDate = appointmentDate,
                QueueNumber = queueNumber,
                Status = AppointmentStatus.Pending
            };

            db.Appointments.Add(appointment);
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Appointment created: {AppointmentId} {AppointmentNumber} Patient={PatientId} Doctor={DoctorId} Date={AppointmentDate} Queue={QueueNumber} by {UserId}",
                appointment.Id, appointmentNumber, request.PatientId, request.DoctorId, appointmentDate, queueNumber, currentUser.UserId);

            // Load response with related data
            var response = await db.Appointments
                .Where(a => a.Id == appointment.Id)
                .Select(a => new Response(
                    a.Id,
                    a.PatientId,
                    a.Patient.FullName,
                    a.DoctorId,
                    a.DoctorProfile.Staff.User.FirstName + " " + a.DoctorProfile.Staff.User.LastName,
                    a.ClinicBranchId,
                    a.ClinicBranch.Name,
                    a.AppointmentTypeId,
                    a.AppointmentType.NameEn,
                    a.AppointmentDate,
                    a.QueueNumber,
                    a.Status,
                    a.CreatedAt
                ))
                .FirstAsync(ct);

            return Results.Created($"/api/appointments/{response.Id}", response);
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    private static async Task<string> GenerateAppointmentNumberAsync(
        ApplicationDbContext db,
        DateTimeProvider dateTimeProvider,
        Guid clinicBranchId,
        CancellationToken ct)
    {
        var year = dateTimeProvider.UtcNow.Year;
        var count = await db.Appointments
            .CountAsync(a => a.ClinicBranchId == clinicBranchId && a.CreatedAt.Year == year, ct);
        
        return $"APT-{year}-{(count + 1):D6}";
    }

    private static async Task<short> GetNextQueueNumberAsync(
        ApplicationDbContext db,
        Guid clinicBranchId,
        DateTime date,
        CancellationToken ct)
    {
        var maxQueue = await db.Appointments
            .Where(a => a.ClinicBranchId == clinicBranchId && a.AppointmentDate == date)
            .MaxAsync(a => (short?)a.QueueNumber, ct);
        
        return (short)((maxQueue ?? 0) + 1);
    }

    public record Request(
        [Required]
        Guid PatientId,
        
        [Required]
        Guid DoctorId,
        
        [Required]
        Guid ClinicBranchId,
        
        [Required]
        [CustomValidation(typeof(CustomValidators), nameof(CustomValidators.MustBeInFutureOrToday))]
        DateTime ScheduledAt,
        
        [Required]
        Guid AppointmentTypeId);

    public record Response(
        Guid Id,
        Guid PatientId,
        string PatientName,
        Guid DoctorId,
        string DoctorName,
        Guid ClinicBranchId,
        string BranchName,
        Guid AppointmentTypeId,
        string AppointmentTypeName,
        DateTime AppointmentDate,
        short QueueNumber,
        AppointmentStatus Status,
        DateTime CreatedAt);
}
