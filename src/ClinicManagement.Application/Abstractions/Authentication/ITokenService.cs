using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Authentication;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles, int? clinicId = null);
}
