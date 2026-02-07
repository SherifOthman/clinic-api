using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Queries.ValidatePhone;

public class ValidatePhoneQueryHandler : IRequestHandler<ValidatePhoneQuery, Result<ValidatePhoneResponse>>
{
    private readonly IPhoneValidationService _phoneValidationService;

    public ValidatePhoneQueryHandler(IPhoneValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;
    }

    public Task<Result<ValidatePhoneResponse>> Handle(ValidatePhoneQuery request, CancellationToken cancellationToken)
    {
        var result = _phoneValidationService.ValidatePhoneNumber(request.PhoneNumber, request.CountryCode);
        return Task.FromResult(Result<ValidatePhoneResponse>.Ok(result));
    }
}
