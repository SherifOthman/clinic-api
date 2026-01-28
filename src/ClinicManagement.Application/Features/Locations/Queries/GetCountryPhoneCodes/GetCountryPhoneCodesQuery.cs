using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.GetCountryPhoneCodes;

public record GetCountryPhoneCodesQuery : IRequest<Result<List<CountryPhoneCodeDto>>>;

public class CountryPhoneCodeDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // ISO2 code
    public string PhoneCode { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;
}