using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public class CheckUsernameAvailabilityHandler : IRequestHandler<CheckUsernameAvailabilityQuery, AvailabilityDto>
{
    private readonly IUnitOfWork _uow;

    public CheckUsernameAvailabilityHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AvailabilityDto> Handle(CheckUsernameAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return new AvailabilityDto(false, "Username is required");

        var exists      = await _uow.Users.AnyByNormalizedUsernameAsync(request.Username.ToUpperInvariant(), cancellationToken);
        var isAvailable = !exists;

        return new AvailabilityDto(isAvailable, isAvailable ? null : "Username is already taken");
    }
}
