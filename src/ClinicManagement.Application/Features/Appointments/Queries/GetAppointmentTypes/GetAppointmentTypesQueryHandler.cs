using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentTypes;

public class GetAppointmentTypesQueryHandler : IRequestHandler<GetAppointmentTypesQuery, Result<IEnumerable<AppointmentTypeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAppointmentTypesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<AppointmentTypeDto>>> Handle(GetAppointmentTypesQuery request, CancellationToken cancellationToken)
    {
        var appointmentTypes = await _unitOfWork.AppointmentTypes.GetActiveAsync();
        var appointmentTypeDtos = appointmentTypes.Adapt<IEnumerable<AppointmentTypeDto>>();
        return Result<IEnumerable<AppointmentTypeDto>>.Ok(appointmentTypeDtos);
    }
}
