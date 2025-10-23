using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Commands.UpdateAppointment;

public class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateAppointmentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<AppointmentDto>> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var appointment = await _context.UnitOfWork.Appointments.GetByIdAsync(request.Id, cancellationToken);
            if (appointment == null)
                return Result<AppointmentDto>.Failure("Appointment not found");

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

            _context.UnitOfWork.Appointments.Update(appointment);
            await _context.SaveChangesAsync(cancellationToken);

            var appointmentDto = _mapper.Map<AppointmentDto>(appointment);
            return Result<AppointmentDto>.Success(appointmentDto);
        }
        catch (Exception ex)
        {
            return Result<AppointmentDto>.Failure(ex.Message);
        }
    }
}
