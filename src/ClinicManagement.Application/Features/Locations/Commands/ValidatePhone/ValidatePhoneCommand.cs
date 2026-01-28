using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Locations.Commands.ValidatePhone;

public record ValidatePhoneCommand(string PhoneNumber) : IRequest<Result<ValidatePhoneResult>>;

public class ValidatePhoneResult
{
    public bool IsValid { get; set; }
    public string Formatted { get; set; } = string.Empty;
    public string OriginalNumber { get; set; } = string.Empty;
}
