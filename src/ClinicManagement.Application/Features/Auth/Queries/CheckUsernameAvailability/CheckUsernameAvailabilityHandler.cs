using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Auth.Queries.CheckUsernameAvailability;

public class CheckUsernameAvailabilityHandler : IRequestHandler<CheckUsernameAvailabilityQuery, CheckUsernameAvailabilityDto>
{
    private readonly UserManager<User> _userManager;

    public CheckUsernameAvailabilityHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<CheckUsernameAvailabilityDto> Handle(
        CheckUsernameAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new CheckUsernameAvailabilityDto(false, "Username is required");
        }

        var user = await _userManager.FindByNameAsync(request.Username);
        var isAvailable = user == null;

        return new CheckUsernameAvailabilityDto(
            isAvailable,
            isAvailable ? null : "Username is already taken"
        );
    }
}
