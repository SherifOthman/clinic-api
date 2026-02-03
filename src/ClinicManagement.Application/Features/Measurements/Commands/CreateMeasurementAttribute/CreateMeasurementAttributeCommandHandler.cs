using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;

public class CreateMeasurementAttributeCommandHandler : IRequestHandler<CreateMeasurementAttributeCommand, Result<Guid>>
{
    private readonly IApplicationDbContext _context;

    public CreateMeasurementAttributeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateMeasurementAttributeCommand request, CancellationToken cancellationToken)
    {
        // Check if measurement attribute with same name already exists
        var existingAttribute = await _context.MeasurementAttributes
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        
        if (existingAttribute != null)
        {
            return Result<Guid>.Fail(MessageCodes.Measurement.ATTRIBUTE_ALREADY_EXISTS);
        }

        var attribute = new MeasurementAttribute
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            DataType = request.DataType
        };

        _context.MeasurementAttributes.Add(attribute);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(attribute.Id);
    }
}
