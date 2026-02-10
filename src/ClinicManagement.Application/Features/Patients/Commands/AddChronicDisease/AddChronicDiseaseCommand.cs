using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.AddChronicDisease;

public record AddChronicDiseaseCommand(
    Guid PatientId,
    CreatePatientChronicDiseaseDto ChronicDisease
) : IRequest<Result<PatientChronicDiseaseDto>>;


public class AddChronicDiseaseCommandHandler : IRequestHandler<AddChronicDiseaseCommand, Result<PatientChronicDiseaseDto>>
{
    private readonly IApplicationDbContext _context;

    public AddChronicDiseaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientChronicDiseaseDto>> Handle(AddChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        // Check if chronic disease exists
        var chronicDisease = await _context.ChronicDiseases.FindAsync(new object[] { request.ChronicDisease.ChronicDiseaseId }, cancellationToken);
        if (chronicDisease == null)
        {
            return Result<PatientChronicDiseaseDto>.FailField("chronicDisease.chronicDiseaseId", MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        // Check if the relationship already exists
        var exists = await _context.PatientChronicDiseases
            .AnyAsync(pcd => pcd.PatientId == request.PatientId && pcd.ChronicDiseaseId == request.ChronicDisease.ChronicDiseaseId, cancellationToken);

        if (exists)
        {
            return Result<PatientChronicDiseaseDto>.FailField("chronicDisease.chronicDiseaseId", MessageCodes.Business.CHRONIC_DISEASE_ALREADY_EXISTS);
        }

        // Create the relationship - using the current entity structure
        var PatientChronicDisease = new PatientChronicDisease
        {
            PatientId = request.PatientId,
            ChronicDiseaseId = request.ChronicDisease.ChronicDiseaseId
        };

        _context.PatientChronicDiseases.Add(PatientChronicDisease);
        await _context.SaveChangesAsync(cancellationToken);

        // Get the created entity with navigation properties
        var createdEntity = await _context.PatientChronicDiseases
            .FirstOrDefaultAsync(pcd => pcd.PatientId == request.PatientId && pcd.ChronicDiseaseId == request.ChronicDisease.ChronicDiseaseId, cancellationToken);

        // Map to DTO with the available properties
        var dto = new PatientChronicDiseaseDto
        {
            Id = createdEntity!.Id,
            PatientId = createdEntity.PatientId,
            ChronicDiseaseId = createdEntity.ChronicDiseaseId,
            DiagnosedDate = request.ChronicDisease.DiagnosedDate,
            Status = request.ChronicDisease.Status,
            Notes = request.ChronicDisease.Notes,
            IsActive = request.ChronicDisease.IsActive,
            CreatedAt = createdEntity.CreatedAt,
            UpdatedAt = createdEntity.UpdatedAt
        };
        
        return Result<PatientChronicDiseaseDto>.Ok(dto);
    }
}