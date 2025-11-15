using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Commands.CreateDoctor;

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Result<DoctorDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDoctorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DoctorDto>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = new Doctor
        {
            UserId = request.UserId,
            SpecializationId = request.SpecializationId,
            Bio = request.Bio,
            IsActive = true
        };

        _unitOfWork.Doctors.Add(doctor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdDoctor = await _unitOfWork.Doctors.GetWithSpecializationAsync(doctor.Id, cancellationToken);
        
        if (createdDoctor == null)
            return Result<DoctorDto>.Fail("Failed to create doctor");

        var doctorDto = new DoctorDto
        {
            Id = createdDoctor.Id,
            UserId = createdDoctor.UserId,
            SpecializationId = createdDoctor.SpecializationId,
            Bio = createdDoctor.Bio,
            IsActive = createdDoctor.IsActive,
            User = createdDoctor.User != null ? new UserDto { FirstName = createdDoctor.User.FirstName, LastName = createdDoctor.User.LastName, Email = createdDoctor.User.Email ?? string.Empty } : null,
            Specialization = createdDoctor.Specialization != null ? new SpecializationDto { Id = createdDoctor.Specialization.Id, Name = createdDoctor.Specialization.Name } : null
        };

        return Result<DoctorDto>.Ok(doctorDto);
    }
}
