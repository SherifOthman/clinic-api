using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand(Guid Id, UpdatePatientDto Dto) : IRequest<Result<PatientDto>>;

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdatePatientCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PatientDto>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var clinicId = _currentUserService.ClinicId!.Value;

        // Get patient with related data
        var patient = await _context.Patients
            .Include(p => p.PhoneNumbers)
            .Include(p => p.ChronicDiseases)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.ClinicId == clinicId, cancellationToken);

        if (patient == null)
        {
            return Result<PatientDto>.Fail(MessageCodes.Patient.NOT_FOUND);
        }

        // Update basic info
        patient.FullName = dto.FullName;
        patient.Gender = dto.Gender;
        patient.DateOfBirth = dto.DateOfBirth;
        patient.CityGeoNameId = dto.CityGeoNameId;

        // Update phone numbers
        // Remove old ones
        patient.PhoneNumbers.Clear();

        // Add new ones
        foreach (var phoneDto in dto.PhoneNumbers)
        {
            patient.PhoneNumbers.Add(new PatientPhone
            {
                PhoneNumber = phoneDto.PhoneNumber,
                IsPrimary = phoneDto.IsPrimary
            });
        }

        // Update chronic diseases
        // Remove old ones
        patient.ChronicDiseases.Clear();

        // Add new ones
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

        await _context.SaveChangesAsync(cancellationToken);

        // Reload with fresh data
        var updatedPatient = await _context.Patients
            .Include(p => p.PhoneNumbers)
            .Include(p => p.ChronicDiseases)
                .ThenInclude(pcd => pcd.ChronicDisease)
            .FirstAsync(p => p.Id == patient.Id, cancellationToken);

        var patientDto = updatedPatient.Adapt<PatientDto>();
        return Result<PatientDto>.Ok(patientDto);
    }
}
