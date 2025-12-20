using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateAppointmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = new Appointment
            {
                BranchId = request.BranchId,
                PatientId = request.PatientId,
                DoctorId = request.DoctorId,
                ReceptionistId = request.ReceptionistId,
                Type = request.Type,
                AppointmentDate = request.AppointmentDate,
                Price = request.Price,
                PaidPrice = request.PaidPrice ?? 0,
                Discount = request.Discount ?? 0,
                Notes = request.Notes,
                Status = Domain.Common.Enums.AppointmentStatus.Scheduled
            };

            _unitOfWork.Appointments.Add(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var appointmentDto = _mapper.Map<AppointmentDto>(appointment);
            return Result<AppointmentDto>.Ok(appointmentDto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Fail(ex.Message);
        }
    }
}
