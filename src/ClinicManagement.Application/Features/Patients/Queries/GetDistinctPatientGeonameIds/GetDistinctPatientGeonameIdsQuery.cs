using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetDistinctPatientCountryIdsQuery(bool IsSuperAdmin = false) : IRequest<Result<List<int>>>;
public record GetDistinctPatientStateIdsQuery(bool IsSuperAdmin = false) : IRequest<Result<List<int>>>;
public record GetDistinctPatientCityIdsQuery(bool IsSuperAdmin = false) : IRequest<Result<List<int>>>;
