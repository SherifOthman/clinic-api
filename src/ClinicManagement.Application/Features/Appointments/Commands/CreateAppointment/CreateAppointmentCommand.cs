using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand(
    CreateAppointmentDto Appointment
) : IRequest<Result<AppointmentDto>>;


public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly ILogger<CreateAppointmentCommandHandler> _logger;

    public CreateAppointmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICodeGeneratorService codeGeneratorService,
        ILogger<CreateAppointmentCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _codeGeneratorService = codeGeneratorService;
        _logger = logger;
    }

    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.GetRequiredClinicId();
        
        // Validate clinic branch exists and belongs to current clinic
        var clinicBranch = await _context.ClinicBranches.FindAsync(new object[] { request.Appointment.ClinicBranchId }, cancellationToken);
        if (clinicBranch == null)
        {
            return Result<AppointmentDto>.Fail(MessageCodes.Common.CLINIC_BRANCH_NOT_FOUND);
        }

        // Validate clinic patient exists and belongs to current clinic
        var Patient = await _context.Patients.FindAsync(new object[] { request.Appointment.PatientId }, cancellationToken);
        if (Patient == null)
        {
            return Result<AppointmentDto>.FailField("appointment.PatientId", MessageCodes.Appointment.PATIENT_REQUIRED);
        }

        // Get the next queue number for the doctor on the appointment date
        var appointmentDate = request.Appointment.AppointmentDate.Date;
        var maxQueueNumber = await _context.Appointments
            .Where(a => a.DoctorId == request.Appointment.DoctorId && a.AppointmentDate.Date == appointmentDate)
            .MaxAsync(a => (int?)a.QueueNumber, cancellationToken) ?? 0;
        
        var queueNumber = maxQueueNumber + 1;

        // Check for conflicts
        var hasConflict = await _context.Appointments
            .AnyAsync(a => a.DoctorId == request.Appointment.DoctorId && 
                          a.AppointmentDate == request.Appointment.AppointmentDate && 
                          a.QueueNumber == queueNumber, 
                     cancellationToken);

        if (hasConflict)
        {
            return Result<AppointmentDto>.Fail(MessageCodes.Appointment.QUEUE_NUMBER_CONFLICT);
        }

        // Get the price for this appointment type at this clinic branch
        var pricing = await _context.ClinicBranchAppointmentPrices
            .FirstOrDefaultAsync(p => p.ClinicBranchId == request.Appointment.ClinicBranchId && 
                                     p.AppointmentTypeId == request.Appointment.AppointmentTypeId, 
                                cancellationToken);

        if (pricing == null)
        {
            return Result<AppointmentDto>.FailField("appointment.appointmentTypeId", MessageCodes.Appointment.TYPE_REQUIRED);
        }

        // Use custom price if provided, otherwise use the default price
        var finalPrice = request.Appointment.CustomPrice ?? pricing.Price;

        // Generate appointment number
        var appointmentNumber = await _codeGeneratorService.GenerateAppointmentNumberAsync(clinicId, cancellationToken);

        // Create the appointment
        var appointment = new Appointment
        {
            AppointmentNumber = appointmentNumber,
            ClinicBranchId = request.Appointment.ClinicBranchId,
            PatientId = request.Appointment.PatientId,
            DoctorId = request.Appointment.DoctorId,
            AppointmentDate = request.Appointment.AppointmentDate,
            QueueNumber = (short)queueNumber,
            Status = AppointmentStatus.Pending,
            AppointmentTypeId = request.Appointment.AppointmentTypeId,
            FinalPrice = finalPrice,
            DiscountAmount = request.Appointment.DiscountAmount,
            PaidAmount = request.Appointment.PaidAmount
        };

        _context.Appointments.Add(appointment);
        
        // Single SaveChanges - atomic transaction
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Appointment created successfully: {AppointmentNumber} for clinic {ClinicId}", 
            appointmentNumber, clinicId);

        // Return DTO projection
        var dto = new AppointmentDto
        {
            Id = appointment.Id,
            AppointmentNumber = appointment.AppointmentNumber,
            AppointmentDate = appointment.AppointmentDate,
            QueueNumber = appointment.QueueNumber,
            Status = appointment.Status,
            FinalPrice = appointment.FinalPrice,
            DiscountAmount = appointment.DiscountAmount,
            PaidAmount = appointment.PaidAmount,
            RemainingAmount = appointment.RemainingAmount,
            ClinicBranchId = appointment.ClinicBranchId,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentTypeId = appointment.AppointmentTypeId
        };
        
        return Result<AppointmentDto>.Ok(dto);
    }
}