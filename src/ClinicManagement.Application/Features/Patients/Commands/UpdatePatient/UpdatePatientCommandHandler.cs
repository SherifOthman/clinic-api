using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePatientCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _unitOfWork.Patients.GetByIdAsync(request.Id, cancellationToken);
            if (patient == null)
                return Result<PatientDto>.Fail("Patient not found");

            patient.Avatar = request.Avatar;
            patient.FirstName = request.FirstName;
            patient.SecondName = request.SecondName;
            patient.ThirdName = request.ThirdName;
            patient.DateOfBirth = request.DateOfBirth;
            patient.Gender = request.Gender;
            patient.City = request.City;
            patient.PhoneNumber = request.PhoneNumber;
            patient.EmergencyContactName = request.EmergencyContactName;
            patient.EmergencyPhone = request.EmergencyPhone;
            patient.GeneralNotes = request.GeneralNotes;
            patient.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Patients.Update(patient);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var patientDto = _mapper.Map<PatientDto>(patient);
            return Result<PatientDto>.Ok(patientDto);
        }
        catch (Exception ex)
        {
            return Result<PatientDto>.Fail(ex.Message);
        }
    }
}
