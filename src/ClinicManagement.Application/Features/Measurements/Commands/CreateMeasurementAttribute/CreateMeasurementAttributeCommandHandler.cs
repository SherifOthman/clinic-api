using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;

public class CreateMeasurementAttributeCommandHandler : IRequestHandler<CreateMeasurementAttributeCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateMeasurementAttributeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateMeasurementAttributeCommand request, CancellationToken cancellationToken)
    {
        // Check if measurement attribute with same name already exists
        var exists = await _unitOfWork.MeasurementAttributes.ExistsByNameAsync(request.NameEn, request.NameAr, cancellationToken);
        
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

        await _unitOfWork.MeasurementAttributes.AddAsync(attribute, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(attribute.Id);
    }
}
