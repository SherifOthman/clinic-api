using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries.GetChronicDiseases;

public record GetChronicDiseasesQuery(Guid PatientId) : IRequest<Result<IEnumerable<PatientChronicDiseaseDto>>>;

public class GetChronicDiseasesQueryHandler : IRequestHandler<GetChronicDiseasesQuery, Result<IEnumerable<PatientChronicDiseaseDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetChronicDiseasesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<PatientChronicDiseaseDto>>> Handle(GetChronicDiseasesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PatientChronicDiseases
            .AsNoTracking()
            .Where(pcd => pcd.PatientId == request.PatientId);

        // Note: IsActive filter would require additional properties on the entity
        // Currently PatientChronicDisease is a simple junction table
        
        var chronicDiseases = await query.ToListAsync(cancellationToken);
        var dtos = chronicDiseases.Adapt<IEnumerable<PatientChronicDiseaseDto>>();
        
        return Result<IEnumerable<PatientChronicDiseaseDto>>.Ok(dtos);
    }
}