using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public class CheckUsernameAvailabilityHandler : IRequestHandler<CheckUsernameAvailabilityQuery, Result<AvailabilityDto>>
{
    private readonly IUserRepository _users;

    public CheckUsernameAvailabilityHandler(IUserRepository users) => _users = users;

    public async Task<Result<AvailabilityDto>> Handle(CheckUsernameAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return Result.Success(new AvailabilityDto(false, "Username is required"));

        var exists      = await _users.AnyByNormalizedUsernameAsync(request.Username.ToUpperInvariant(), cancellationToken);
        var isAvailable = !exists;

        return Result.Success(new AvailabilityDto(isAvailable, isAvailable ? null : "Username is already taken"));
    }
}
