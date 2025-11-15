using AutoMapper;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointments;

public class GetAppointmentsQueryHandler : IRequestHandler<GetAppointmentsQuery, Result<PaginatedList<AppointmentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAppointmentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedList<AppointmentDto>>> Handle(GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await _unitOfWork.Appointments.GetAppointmentsPagedAsync(
            request.BranchId,
            request.PatientId,
            request.DoctorId,
            request.Status,
            request.Type,
            request.FromDate,
            request.ToDate,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var appointmentDtos = _mapper.Map<List<AppointmentDto>>(appointments);
        
        var result = new PaginatedList<AppointmentDto>(
            appointmentDtos,
            appointmentDtos.Count,
            request.PageNumber,
            request.PageSize
        );

        return Result<PaginatedList<AppointmentDto>>.Ok(result);
    }
}
