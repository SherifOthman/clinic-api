using ClinicManagement.API.Common.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Common.Extensions;

public static class IdentityResultExtensions
{
    /// <summary>
    /// Throws DomainValidationException if IdentityResult failed
    /// </summary>
    public static void ThrowIfFailed(this IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
            return;

        var errors = new Dictionary<string, List<string>>();
        foreach (var error in identityResult.Errors)
        {
            if (!errors.ContainsKey(error.Code))
                errors[error.Code] = new List<string>();
            errors[error.Code].Add(error.Description);
        }

        throw new DomainValidationException(errors);
    }
}
