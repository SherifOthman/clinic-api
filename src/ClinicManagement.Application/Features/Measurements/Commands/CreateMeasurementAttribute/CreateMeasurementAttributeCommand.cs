using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;

public record CreateMeasurementAttributeCommand : IRequest<Result<Guid>>
{
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? DescriptionEn { get; init; }
    public string? DescriptionAr { get; init; }
    public MeasurementDataType DataType { get; init; }
}
