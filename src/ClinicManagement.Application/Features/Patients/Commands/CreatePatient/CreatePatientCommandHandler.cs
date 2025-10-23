using AutoMapper;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreatePatientCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var patient = new Patient
            {
                ClinicId = request.ClinicId,
                Avatar = request.Avatar,
                FirstName = request.FirstName,
                SecondName = request.SecondName,
                ThirdName = request.ThirdName,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                City = request.City,
                PhoneNumber = request.PhoneNumber,
                EmergencyContactName = request.EmergencyContactName,
                EmergencyPhone = request.EmergencyPhone,
                GeneralNotes = request.GeneralNotes
            };

            _context.UnitOfWork.Patients.Add(patient);
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
