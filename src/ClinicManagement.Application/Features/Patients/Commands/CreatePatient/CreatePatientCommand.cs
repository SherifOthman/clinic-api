using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand(CreatePatientDto Dto) : IRequest<Result<PatientDto>>;

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICodeGeneratorService _codeGenerator;

    public CreatePatientCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ICodeGeneratorService codeGenerator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _codeGenerator = codeGenerator;
    }

    public async Task<Result<PatientDto>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var clinicId = _currentUserService.ClinicId!.Value;

        // Generate unique patient code
        var patientCode = await _codeGenerator.GeneratePatientNumberAsync(clinicId, cancellationToken);

        // Create patient
        var patient = new Patient
        {
            PatientCode = patientCode,
            ClinicId = clinicId,
            FullName = dto.FullName,
            Gender = dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            CityGeoNameId = dto.CityGeoNameId
        };

        // Add phone numbers
        foreach (var phoneDto in dto.PhoneNumbers)
        {
            patient.PhoneNumbers.Add(new PatientPhone
            {
                PhoneNumber = phoneDto.PhoneNumber,
                IsPrimary = phoneDto.IsPrimary
            });
        }

        // Add chronic diseases
        if (dto.ChronicDiseaseIds.Any())
        {
            var chronicDiseases = await _context.ChronicDiseases
                .Where(cd => dto.ChronicDiseaseIds.Contains(cd.Id))
                .ToListAsync(cancellationToken);

            foreach (var disease in chronicDiseases)
            {
                patient.ChronicDiseases.Add(new PatientChronicDisease
                {
                    ChronicDiseaseId = disease.Id
                });
            }
        }

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync(cancellationToken);

        // Load related data for response
        var createdPatient = await _context.Patients
            .Include(p => p.PhoneNumbers)
            .Include(p => p.ChronicDiseases)
                .ThenInclude(pcd => pcd.ChronicDisease)
            .FirstAsync(p => p.Id == patient.Id, cancellationToken);

        var patientDto = createdPatient.Adapt<PatientDto>();
        return Result<PatientDto>.Ok(patientDto);
    }
}
