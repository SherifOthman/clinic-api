using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Measurements.Queries.GetMeasurementAttributes;

public class GetMeasurementAttributesQueryHandler : IRequestHandler<GetMeasurementAttributesQuery, Result<IEnumerable<MeasurementAttributeDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetMeasurementAttributesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<MeasurementAttributeDto>>> Handle(GetMeasurementAttributesQuery request, CancellationToken cancellationToken)
    {
        var attributes = await _context.MeasurementAttributes.ToListAsync(cancellationToken);
        var attributesDto = attributes.Adapt<IEnumerable<MeasurementAttributeDto>>();
        
        return Result<IEnumerable<MeasurementAttributeDto>>.Ok(attributesDto);
    }
}
