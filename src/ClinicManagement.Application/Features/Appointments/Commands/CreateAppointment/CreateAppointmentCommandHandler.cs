using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateAppointmentCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
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

            _context.UnitOfWork.Appointments.Add(appointment);
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
