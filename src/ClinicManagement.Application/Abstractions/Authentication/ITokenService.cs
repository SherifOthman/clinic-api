using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Abstractions.Authentication;

public interface ITokenService
{
    string GenerateAccessToken(
        User user,
        IEnumerable<string> roles,
        IEnumerable<Permission> permissions,
        Guid? clinicId = null,
        string? countryCode = null);
}
