using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Common.Enums;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClinicBranchAppointmentPriceRepository _clinicBranchAppointmentPriceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IClinicBranchAppointmentPriceRepository clinicBranchAppointmentPriceRepository,
        IUnitOfWork unitOfWork)
    {
        _appointmentRepository = appointmentRepository;
        _clinicBranchAppointmentPriceRepository = clinicBranchAppointmentPriceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Validate clinic branch exists
        var clinicBranchExists = await _unitOfWork.ClinicBranches.ExistsAsync(request.Appointment.ClinicBranchId, cancellationToken);
        if (!clinicBranchExists)
        {
            return Result<AppointmentDto>.Fail(MessageCodes.Common.CLINIC_BRANCH_NOT_FOUND);
        }

        // Validate clinic patient exists
        var clinicPatientExists = await _unitOfWork.ClinicPatients.ExistsAsync(request.Appointment.ClinicPatientId, cancellationToken);
        if (!clinicPatientExists)
        {
            return Result<AppointmentDto>.FailField("appointment.clinicPatientId", MessageCodes.Appointment.PATIENT_REQUIRED);
        }

        // Get the next queue number for the doctor on the appointment date
        var queueNumber = await _appointmentRepository.GetNextQueueNumberAsync(
            request.Appointment.AppointmentDate.Date, 
            request.Appointment.DoctorId, 
            cancellationToken);

        // Get the price for this appointment type at this clinic branch
        var pricing = await _clinicBranchAppointmentPriceRepository.GetPriceAsync(
            request.Appointment.ClinicBranchId,
            request.Appointment.AppointmentTypeId,
            cancellationToken);

        if (pricing == null)
        {
            return Result<AppointmentDto>.FailField("appointment.appointmentTypeId", MessageCodes.Appointment.TYPE_REQUIRED);
        }

        // Use custom price if provided, otherwise use the default price
        var finalPrice = request.Appointment.CustomPrice ?? pricing.Value;

        // Create the appointment
        var appointment = new Appointment
        {
            ClinicBranchId = request.Appointment.ClinicBranchId,
            ClinicPatientId = request.Appointment.ClinicPatientId,
            DoctorId = request.Appointment.DoctorId,
            AppointmentDate = request.Appointment.AppointmentDate,
            QueueNumber = queueNumber,
            Status = AppointmentStatus.Pending,
            AppointmentTypeId = request.Appointment.AppointmentTypeId,
            FinalPrice = finalPrice,
            DiscountAmount = request.Appointment.DiscountAmount,
            PaidAmount = request.Appointment.PaidAmount
        };

        await _appointmentRepository.AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get the created appointment with navigation properties
        var createdAppointment = await _appointmentRepository.GetByIdAsync(appointment.Id, cancellationToken);
        var dto = createdAppointment!.Adapt<AppointmentDto>();
        
        return Result<AppointmentDto>.Ok(dto);
    }
}
