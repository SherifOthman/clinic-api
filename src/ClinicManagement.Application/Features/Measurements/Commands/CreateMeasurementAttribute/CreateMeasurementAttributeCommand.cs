using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;

public record CreateMeasurementAttributeCommand : IRequest<Result<Guid>>
{
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? DescriptionEn { get; init; }
    public string? DescriptionAr { get; init; }
    public MeasurementDataType DataType { get; init; }
}


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
        var query = _context.MeasurementAttributes.AsQueryable();
        
        bool exists;
        if (!string.IsNullOrEmpty(request.NameAr))
        {
            exists = await query.AnyAsync(m => 
                m.NameEn.ToLower() == request.NameEn.ToLower() || 
                m.NameAr.ToLower() == request.NameAr.ToLower(), 
                cancellationToken);
        }
        else
        {
            exists = await query.AnyAsync(m => 
                m.NameEn.ToLower() == request.NameEn.ToLower(), 
                cancellationToken);
        }
        
        if (exists)
        {
            return Result<Guid>.FailField("nameEn", MessageCodes.Measurement.ATTRIBUTE_ALREADY_EXISTS);
        }

        var attribute = new MeasurementAttribute
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            DescriptionEn = request.DescriptionEn,
            DescriptionAr = request.DescriptionAr,
            DataType = request.DataType
        };

        _context.MeasurementAttributes.Add(attribute);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(attribute.Id);
    }
}