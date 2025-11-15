using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctorById;

public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, Result<DoctorDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDoctorByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DoctorDto>> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _unitOfWork.Doctors.GetWithSpecializationAsync(request.Id, cancellationToken);

        if (doctor == null)
            return Result<DoctorDto>.Fail("Doctor not found");

        var doctorDto = new DoctorDto
        {
            Id = doctor.Id,
            UserId = doctor.UserId,
            SpecializationId = doctor.SpecializationId,
            Bio = doctor.Bio,
            IsActive = doctor.IsActive,
            User = doctor.User != null ? new UserDto { FirstName = doctor.User.FirstName, LastName = doctor.User.LastName, Email = doctor.User.Email ?? string.Empty } : null,
            Specialization = doctor.Specialization != null ? new SpecializationDto { Id = doctor.Specialization.Id, Name = doctor.Specialization.Name } : null
        };

        return Result<DoctorDto>.Ok(doctorDto);
    }
}
