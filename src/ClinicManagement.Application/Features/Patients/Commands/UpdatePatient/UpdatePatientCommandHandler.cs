using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdatePatientCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = await _context.UnitOfWork.Patients.GetByIdAsync(request.Id, cancellationToken);
            if (patient == null)
                return Result<PatientDto>.Failure("Patient not found");

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

            _context.UnitOfWork.Patients.Update(patient);
            await _context.SaveChangesAsync(cancellationToken);

            var patientDto = _mapper.Map<PatientDto>(patient);
            return Result<PatientDto>.Success(patientDto);
        }
        catch (Exception ex)
        {
            return Result<PatientDto>.Failure(ex.Message);
        }
    }
}
