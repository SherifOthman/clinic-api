using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Specializations.Queries.GetSpecializations;

public class GetSpecializationsHandler : IRequestHandler<GetSpecializationsQuery, List<SpecializationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSpecializationsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SpecializationDto>> Handle(GetSpecializationsQuery request, CancellationToken cancellationToken)
    {
        var specializations = await _context.Set<Specialization>()
            .OrderBy(s => s.NameEn)
            .Select(s => new SpecializationDto(
                s.Id,
                s.NameEn,
                s.NameAr
            ))
            .ToListAsync(cancellationToken);

        return specializations;
    }
}
