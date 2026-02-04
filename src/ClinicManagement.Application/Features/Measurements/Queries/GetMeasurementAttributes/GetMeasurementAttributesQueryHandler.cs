using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Measurements.Queries.GetMeasurementAttributes;

public class GetMeasurementAttributesQueryHandler : IRequestHandler<GetMeasurementAttributesQuery, Result<IEnumerable<MeasurementAttributeDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMeasurementAttributesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<MeasurementAttributeDto>>> Handle(GetMeasurementAttributesQuery request, CancellationToken cancellationToken)
    {
        var attributes = await _unitOfWork.MeasurementAttributes.GetAllAsync(cancellationToken);
        var attributesDto = attributes.Adapt<IEnumerable<MeasurementAttributeDto>>();
        
        return Result<IEnumerable<MeasurementAttributeDto>>.Ok(attributesDto);
    }
}
