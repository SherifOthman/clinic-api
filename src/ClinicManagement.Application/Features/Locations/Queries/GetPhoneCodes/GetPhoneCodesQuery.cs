using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetPhoneCodes;

public record GetPhoneCodesQuery : IRequest<Result<List<CountryPhoneCodeDto>>>;
