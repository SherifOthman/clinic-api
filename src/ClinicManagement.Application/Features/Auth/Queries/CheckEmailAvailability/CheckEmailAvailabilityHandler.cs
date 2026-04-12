using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public class CheckEmailAvailabilityHandler : IRequestHandler<CheckEmailAvailabilityQuery, AvailabilityDto>
{
    private readonly IUnitOfWork _uow;

    public CheckEmailAvailabilityHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<AvailabilityDto> Handle(CheckEmailAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return new AvailabilityDto(false, "Email is required");

        var exists      = await _uow.Users.AnyByNormalizedEmailAsync(request.Email.ToUpperInvariant(), cancellationToken);
        var isAvailable = !exists;

        return new AvailabilityDto(isAvailable, isAvailable ? null : "Email is already taken");
    }
}
