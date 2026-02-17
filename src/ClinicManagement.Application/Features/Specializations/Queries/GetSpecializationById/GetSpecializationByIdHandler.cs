using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Specializations.Queries.GetSpecializations;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Specializations.Queries.GetSpecializationById;

public class GetSpecializationByIdHandler : IRequestHandler<GetSpecializationByIdQuery, SpecializationDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSpecializationByIdHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SpecializationDto?> Handle(GetSpecializationByIdQuery request, CancellationToken cancellationToken)
    {
        var specialization = await _context.Set<Specialization>()
            .Where(s => s.Id == request.Id)
            .Select(s => new SpecializationDto(
                s.Id,
                s.NameEn,
                s.NameAr
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return specialization;
    }
}
