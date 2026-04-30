using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public class CheckEmailAvailabilityHandler : IRequestHandler<CheckEmailAvailabilityQuery, Result<AvailabilityDto>>
{
    private readonly IUnitOfWork _uow;

    public CheckEmailAvailabilityHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<AvailabilityDto>> Handle(CheckEmailAvailabilityQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Success(new AvailabilityDto(false, "Email is required"));

        var exists      = await _uow.Users.AnyByNormalizedEmailAsync(request.Email.ToUpperInvariant(), cancellationToken);
        var isAvailable = !exists;

        return Result.Success(new AvailabilityDto(isAvailable, isAvailable ? null : "Email is already taken"));
    }
}
