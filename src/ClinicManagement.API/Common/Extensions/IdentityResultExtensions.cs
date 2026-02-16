using ClinicManagement.API.Common.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.API.Common.Extensions;

public static class IdentityResultExtensions
{
    /// <summary>
    /// Throws DomainException if IdentityResult failed
    /// </summary>
    public static void ThrowIfFailed(this IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
            return;

        // Get first error (most relevant)
        var firstError = identityResult.Errors.FirstOrDefault();
        if (firstError != null)
        {
            // Convert Identity error codes to our error codes
            var errorCode = firstError.Code switch
            {
                "DuplicateUserName" => "USERNAME_ALREADY_EXISTS",
                "DuplicateEmail" => "EMAIL_ALREADY_EXISTS",
                "PasswordTooShort" => "INVALID_PASSWORD",
                "PasswordRequiresDigit" => "INVALID_PASSWORD",
                "PasswordRequiresLower" => "INVALID_PASSWORD",
                "PasswordRequiresUpper" => "INVALID_PASSWORD",
                "PasswordRequiresNonAlphanumeric" => "INVALID_PASSWORD",
                "InvalidEmail" => "INVALID_EMAIL",
                "InvalidUserName" => "INVALID_USERNAME",
                _ => "OPERATION_FAILED"
            };

            throw new DomainException(errorCode, firstError.Description);
        }

        throw new DomainException("OPERATION_FAILED", "Operation failed");
    }
}
