using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Measurements.Queries.GetMeasurementAttributes;

public record GetMeasurementAttributesQuery : IRequest<Result<IEnumerable<MeasurementAttributeDto>>>;