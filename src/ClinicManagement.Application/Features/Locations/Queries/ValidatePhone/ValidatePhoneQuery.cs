using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.ValidatePhone;

public record ValidatePhoneQuery(string PhoneNumber, string? CountryCode = null) 
    : IRequest<Result<ValidatePhoneResponse>>;
