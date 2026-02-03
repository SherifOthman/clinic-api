using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using MediatR;

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
        var attribute = new MeasurementAttribute
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            DataType = request.DataType
        };

        _context.MeasurementAttributes.Add(attribute);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(attribute.Id);
    }
}