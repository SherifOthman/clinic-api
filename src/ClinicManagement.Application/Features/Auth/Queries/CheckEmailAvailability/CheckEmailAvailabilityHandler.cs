using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Auth.Queries.CheckEmailAvailability;

public class CheckEmailAvailabilityHandler : IRequestHandler<CheckEmailAvailabilityQuery, CheckEmailAvailabilityDto>
{
    private readonly UserManager<User> _userManager;

    public CheckEmailAvailabilityHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<CheckEmailAvailabilityDto> Handle(
        CheckEmailAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new CheckEmailAvailabilityDto(false, "Email is required");
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        var isAvailable = user == null;

        return new CheckEmailAvailabilityDto(
            isAvailable,
            isAvailable ? null : "Email is already taken"
        );
    }
}
