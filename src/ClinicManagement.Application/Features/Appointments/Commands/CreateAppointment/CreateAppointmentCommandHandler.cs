using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

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
        
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Validate clinic branch exists and belongs to current clinic
            var clinicBranch = await _unitOfWork.ClinicBranches.GetByIdAsync(request.Appointment.ClinicBranchId, cancellationToken);
            if (clinicBranch == null)
            {
                return Result<AppointmentDto>.Fail(MessageCodes.Common.CLINIC_BRANCH_NOT_FOUND);
            }

            // Validate clinic patient exists and belongs to current clinic
            var Patient = await _unitOfWork.Patients.GetByIdAsync(request.Appointment.PatientId, cancellationToken);
            if (Patient == null)
            {
                return Result<AppointmentDto>.FailField("appointment.PatientId", MessageCodes.Appointment.PATIENT_REQUIRED);
            }

            // Get the next queue number for the doctor on the appointment date
            var queueNumber = await _unitOfWork.Appointments.GetNextQueueNumberAsync(
                request.Appointment.AppointmentDate.Date, 
                request.Appointment.DoctorId, 
                cancellationToken);

            // Check for conflicts
            var hasConflict = await _unitOfWork.Appointments.HasConflictingAppointmentAsync(
                request.Appointment.DoctorId,
                request.Appointment.AppointmentDate,
                queueNumber,
                cancellationToken: cancellationToken);

            if (hasConflict)
            {
                return Result<AppointmentDto>.Fail(MessageCodes.Appointment.QUEUE_NUMBER_CONFLICT);
            }

            // Get the price for this appointment type at this clinic branch
            var pricing = await _unitOfWork.ClinicBranchAppointmentPrices.GetPriceAsync(
                request.Appointment.ClinicBranchId,
                request.Appointment.AppointmentTypeId,
                cancellationToken);

            if (pricing == null)
            {
                return Result<AppointmentDto>.FailField("appointment.appointmentTypeId", MessageCodes.Appointment.TYPE_REQUIRED);
            }

            // Use custom price if provided, otherwise use the default price
            var finalPrice = request.Appointment.CustomPrice ?? pricing.Value;

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
                QueueNumber = queueNumber,
                Status = AppointmentStatus.Pending,
                AppointmentTypeId = request.Appointment.AppointmentTypeId,
                FinalPrice = finalPrice,
                DiscountAmount = request.Appointment.DiscountAmount,
                PaidAmount = request.Appointment.PaidAmount
            };

            await _unitOfWork.Appointments.AddAsync(appointment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

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
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating appointment for clinic {ClinicId}", clinicId);
            throw;
        }
    }
}