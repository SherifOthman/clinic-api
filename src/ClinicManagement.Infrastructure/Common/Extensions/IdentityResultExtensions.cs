using ClinicManagement.Application.Common.Models;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Infrastructure.Common.Extensions;

/// <summary>
/// Extension methods for mapping IdentityResult to Result.
/// </summary>
public static class IdentityResultExtensions
{
    /// <summary>
    /// Maps an IdentityResult to a Result object.
    /// </summary>
    /// <param name="identityResult">The IdentityResult to map.</param>
    /// <returns>A Result object containing success status or error details.</returns>
    public static Result ToResult(this IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
            return Result.Ok();

        var validationErrors = new Dictionary<string, List<string>>();
        
        foreach (var error in identityResult.Errors)
        {
            var field = GetFieldNameFromErrorCode(error.Code);
            if (!validationErrors.ContainsKey(field))
                validationErrors[field] = new List<string>();
            validationErrors[field].Add(error.Description);
        }

        return Result.FailValidation(validationErrors);
    }

    /// <summary>
    /// Maps Identity error codes to field names for better error reporting.
    /// </summary>
    /// <param name="errorCode">The Identity error code.</param>
    /// <returns>The corresponding field name, or empty string if not mapped.</returns>
    private static string GetFieldNameFromErrorCode(string errorCode) => errorCode switch
    {
        "DuplicateEmail" or "InvalidEmail" => "email",
        "DuplicateUserName" or "InvalidUserName" => "userName",
        "PasswordTooShort" or "PasswordRequiresDigit" or "PasswordRequiresLower" 
            or "PasswordRequiresUpper" or "PasswordRequiresNonAlphanumeric" => "password",
        _ => "general"
    };
}
