using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdateChronicDisease;

public record UpdateChronicDiseaseCommand(
    Guid PatientId,
    Guid ChronicDiseaseId,
    UpdatePatientChronicDiseaseDto UpdateData
) : IRequest<Result<PatientChronicDiseaseDto>>;

public class UpdateChronicDiseaseCommandHandler : IRequestHandler<UpdateChronicDiseaseCommand, Result<PatientChronicDiseaseDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateChronicDiseaseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PatientChronicDiseaseDto>> Handle(UpdateChronicDiseaseCommand request, CancellationToken cancellationToken)
    {
        var PatientChronicDisease = await _context.PatientChronicDiseases
            .FirstOrDefaultAsync(pcd => pcd.PatientId == request.PatientId && pcd.ChronicDiseaseId == request.ChronicDiseaseId, cancellationToken);

        if (PatientChronicDisease == null)
        {
            return Result<PatientChronicDiseaseDto>.Fail(MessageCodes.Business.CHRONIC_DISEASE_NOT_FOUND);
        }

        // Note: The current PatientChronicDisease entity only has PatientId and ChronicDiseaseId
        // Additional properties like DiagnosedDate, Status, Notes, IsActive are not stored in the entity
        // This is a simple junction table for many-to-many relationship

        _context.PatientChronicDiseases.Update(PatientChronicDisease);
        await _context.SaveChangesAsync(cancellationToken);

        // Get the updated entity with navigation properties
        var updatedEntity = await _context.PatientChronicDiseases
            .FirstOrDefaultAsync(pcd => pcd.PatientId == request.PatientId && pcd.ChronicDiseaseId == request.ChronicDiseaseId, cancellationToken);

        // Map to DTO - since entity doesn't have these properties, we'll use the request data
        var dto = new PatientChronicDiseaseDto
        {
            Id = updatedEntity!.Id,
            PatientId = updatedEntity.PatientId,
            ChronicDiseaseId = updatedEntity.ChronicDiseaseId,
            DiagnosedDate = request.UpdateData.DiagnosedDate,
            Status = request.UpdateData.Status,
            Notes = request.UpdateData.Notes,
            IsActive = request.UpdateData.IsActive,
            CreatedAt = updatedEntity.CreatedAt,
            UpdatedAt = updatedEntity.UpdatedAt
        };
        
        return Result<PatientChronicDiseaseDto>.Ok(dto);
    }
}