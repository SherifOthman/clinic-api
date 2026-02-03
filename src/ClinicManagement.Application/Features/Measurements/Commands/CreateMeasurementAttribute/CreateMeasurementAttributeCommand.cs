using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;

public record CreateMeasurementAttributeCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = null!;
    public MeasurementDataType DataType { get; init; }
}
