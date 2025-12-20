using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateAppointmentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppointmentDto>> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _unitOfWork.Appointments.GetByIdAsync(request.Id, cancellationToken);
            if (appointment == null)
                return Result<AppointmentDto>.Fail("Appointment not found");

            if (request.Status.HasValue)
                appointment.Status = request.Status.Value;
            
            if (request.Type.HasValue)
                appointment.Type = request.Type.Value;
            
            if (request.AppointmentDate.HasValue)
                appointment.AppointmentDate = request.AppointmentDate.Value;
            
            if (request.Price.HasValue)
                appointment.Price = request.Price.Value;
            
            if (request.PaidPrice.HasValue)
                appointment.PaidPrice = request.PaidPrice.Value;
            
            if (request.Discount.HasValue)
                appointment.Discount = request.Discount.Value;
            
            if (request.Notes != null)
                appointment.Notes = request.Notes;

            appointment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Appointments.Update(appointment);
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
