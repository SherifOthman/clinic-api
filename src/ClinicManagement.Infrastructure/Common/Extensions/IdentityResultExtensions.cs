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

        var errors = identityResult.Errors.Select(e => new ErrorItem(
            field: GetFieldNameFromErrorCode(e.Code),
            code: e.Description
        )).ToList();

        return Result.Fail(errors);
    }

    /// <summary>
    /// Maps Identity error codes to field names for better error reporting.
    /// </summary>
    /// <param name="errorCode">The Identity error code.</param>
    /// <returns>The corresponding field name, or empty string if not mapped.</returns>
    private static string GetFieldNameFromErrorCode(string errorCode) => errorCode switch
    {
        "DuplicateEmail" or "InvalidEmail" => "Email",
        "DuplicateUserName" or "InvalidUserName" => "UserName",
        "PasswordTooShort" or "PasswordRequiresDigit" or "PasswordRequiresLower" 
            or "PasswordRequiresUpper" or "PasswordRequiresNonAlphanumeric" => "Password",
        _ => string.Empty
    };
}
