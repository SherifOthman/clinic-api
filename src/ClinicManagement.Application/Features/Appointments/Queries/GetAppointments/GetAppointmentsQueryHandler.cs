using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
        try
        {
            //var query = _unitOfWork.Appointments.GetAll();

            //// Apply filters
            //if (request.BranchId.HasValue)
            //{
            //    query = query.Where(a => a.BranchId == request.BranchId.Value);
            //}

            //if (request.PatientId.HasValue)
            //{
            //    query = query.Where(a => a.PatientId == request.PatientId.Value);
            //}

            //if (request.DoctorId.HasValue)
            //{
            //    query = query.Where(a => a.DoctorId == request.DoctorId.Value);
            //}

            //if (request.Status.HasValue)
            //{
            //    query = query.Where(a => a.Status == request.Status.Value);
            //}

            //if (request.Type.HasValue)
            //{
            //    query = query.Where(a => a.Type == request.Type.Value);
            //}

            //if (request.FromDate.HasValue)
            //{
            //    query = query.Where(a => a.AppointmentDate >= request.FromDate.Value);
            //}

            //if (request.ToDate.HasValue)
            //{
            //    query = query.Where(a => a.AppointmentDate <= request.ToDate.Value);
            //}

            //// Order by appointment date
            //query = query.OrderBy(a => a.AppointmentDate);

            //// Create paginated result
            //var paginatedAppointments = await PaginatedList<Appointment>.CreateAsync(
            //    query, request.PageNumber, request.PageSize, cancellationToken);

            //var appointmentDtos = _mapper.Map<List<AppointmentDto>>(paginatedAppointments.Items);
            
            //var result = new PaginatedList<AppointmentDto>(
            //    appointmentDtos,
            //    paginatedAppointments.TotalCount,
            //    paginatedAppointments.PageNumber,
            //    paginatedAppointments.PageSize
            //);

            return Result<PaginatedList<AppointmentDto>>.Failure("TST");
        }
        catch (Exception ex)
        {
            return Result<PaginatedList<AppointmentDto>>.Failure(ex.Message);
        }
    }
}
