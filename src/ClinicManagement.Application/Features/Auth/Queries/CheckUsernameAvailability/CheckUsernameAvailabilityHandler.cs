using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.CheckUsernameAvailability;

public class CheckUsernameAvailabilityHandler : IRequestHandler<CheckUsernameAvailabilityQuery, CheckUsernameAvailabilityDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CheckUsernameAvailabilityHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CheckUsernameAvailabilityDto> Handle(
        CheckUsernameAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new CheckUsernameAvailabilityDto(false, "Username is required");
        }

        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username, cancellationToken);
        var isAvailable = user == null;

        return new CheckUsernameAvailabilityDto(
            isAvailable,
            isAvailable ? null : "Username is already taken"
        );
    }
}
