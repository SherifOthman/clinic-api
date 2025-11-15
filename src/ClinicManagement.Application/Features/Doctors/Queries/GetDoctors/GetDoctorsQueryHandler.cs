using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctors;

public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery, Result<List<DoctorDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDoctorsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<DoctorDto>>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctors = await _unitOfWork.Doctors.GetDoctorsWithDetailsAsync(request.SpecializationId, cancellationToken);

        var doctorDtos = new List<DoctorDto>();
        
        foreach (var d in doctors)
        {
            doctorDtos.Add(new DoctorDto
            {
                Id = d.Id,
                UserId = d.UserId,
                SpecializationId = d.SpecializationId,
                Bio = d.Bio,
                IsActive = d.IsActive,
                User = d.User != null ? new UserDto { FirstName = d.User.FirstName, LastName = d.User.LastName, Email = d.User.Email ?? string.Empty } : null,
                Specialization = d.Specialization != null ? new SpecializationDto { Id = d.Specialization.Id, Name = d.Specialization.Name } : null
            });
        }

        return Result<List<DoctorDto>>.Ok(doctorDtos);
    }
}
