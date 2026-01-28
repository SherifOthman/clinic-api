using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Commands.ValidatePhone;

public class ValidatePhoneCommandHandler : IRequestHandler<ValidatePhoneCommand, Result<ValidatePhoneResult>>
{
    private readonly IPhoneNumberValidationService _phoneValidationService;

    public ValidatePhoneCommandHandler(IPhoneNumberValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;
    }

    public async Task<Result<ValidatePhoneResult>> Handle(ValidatePhoneCommand request, CancellationToken cancellationToken)
    {
        var isValid = _phoneValidationService.IsValidPhoneNumber(request.PhoneNumber);
        var formatted = _phoneValidationService.FormatPhoneNumber(request.PhoneNumber);

        var result = new ValidatePhoneResult
        {
            IsValid = isValid,
            Formatted = formatted,
            OriginalNumber = request.PhoneNumber
        };

        return Result<ValidatePhoneResult>.Ok(result);
    }
}
