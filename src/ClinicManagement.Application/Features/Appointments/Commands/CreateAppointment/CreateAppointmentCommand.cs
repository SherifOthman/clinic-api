using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICodeGeneratorService _codeGeneratorService;
    private readonly ILogger<CreateAppointmentCommandHandler> _logger;

    public CreateAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ICodeGeneratorService codeGeneratorService,
        ILogger<CreateAppointmentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _codeGeneratorService = codeGeneratorService;
        _logger = logger;
    }

    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.GetRequiredClinicId();
        
        // Validate clinic branch exists and belongs to current clinic
        var clinicBranch = await _unitOfWork.Repository<ClinicBranch>().GetByIdAsync(request.Appointment.ClinicBranchId, cancellationToken);
        if (clinicBranch == null)
        {
            return Result<AppointmentDto>.FailSystem("NOT_FOUND", "Clinic branch not found");
        }

        // Validate clinic patient exists and belongs to current clinic
        var Patient = await _unitOfWork.Patients.GetByIdAsync(request.Appointment.PatientId, cancellationToken);
        if (Patient == null)
        {
            return Result<AppointmentDto>.FailValidation("appointment.PatientId", "Patient is required");
        }

        // Get the next queue number for the doctor on the appointment date
        var queueNumber = await _unitOfWork.Appointments.GetNextQueueNumberAsync(
            request.Appointment.DoctorId, 
            request.Appointment.AppointmentDate.Date, 
            cancellationToken);

        // Check for conflicts
        var hasConflict = await _unitOfWork.Appointments.HasQueueConflictAsync(
            request.Appointment.DoctorId,
            request.Appointment.AppointmentDate,
            queueNumber,
            cancellationToken);

        if (hasConflict)
        {
            return Result<AppointmentDto>.FailBusiness(
                "QUEUE_NUMBER_CONFLICT",
                "This queue number is already taken for the selected doctor and date",
                new { doctorId = request.Appointment.DoctorId, date = request.Appointment.AppointmentDate.Date, queueNumber });
        }

        // Get the price for this appointment type at this clinic branch
        var pricing = await _unitOfWork.Repository<ClinicBranchAppointmentPrice>()
            .FirstOrDefaultAsync(p => p.ClinicBranchId == request.Appointment.ClinicBranchId && 
                                     p.AppointmentTypeId == request.Appointment.AppointmentTypeId, 
                                cancellationToken);

        if (pricing == null)
        {
            return Result<AppointmentDto>.FailValidation("appointment.appointmentTypeId", "Appointment type is required and must have pricing configured");
        }

        // Use custom price if provided, otherwise use the default price
        var finalPrice = request.Appointment.CustomPrice ?? pricing.Price;

        // Generate appointment number
        var appointmentNumber = await _codeGeneratorService.GenerateAppointmentNumberAsync(clinicId, cancellationToken);

        // Create the appointment using factory method (no payment tracking)
        var appointment = Appointment.Create(
            appointmentNumber,
            request.Appointment.ClinicBranchId,
            request.Appointment.PatientId,
            request.Appointment.DoctorId,
            request.Appointment.AppointmentTypeId,
            request.Appointment.AppointmentDate,
            (short)queueNumber
        );

        await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
        
        // TODO: Create invoice for consultation fee if payment is required
        // This should be done in a separate handler or service
        // For now, just create the appointment
        
        // Single SaveChanges - atomic transaction
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            ClinicBranchId = appointment.ClinicBranchId,
            PatientId = appointment.PatientId,
            DoctorId = appointment.DoctorId,
            AppointmentTypeId = appointment.AppointmentTypeId,
            // Payment info will come from linked invoice
            FinalPrice = finalPrice,  // For display only
            DiscountAmount = 0,
            PaidAmount = 0,
            RemainingAmount = finalPrice
        };
        
        return Result<AppointmentDto>.Ok(dto);
    }
}