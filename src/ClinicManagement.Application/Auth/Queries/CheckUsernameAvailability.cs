using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Auth.Queries;

public record CheckUsernameAvailabilityQuery(
    string Username
) : IRequest<CheckUsernameAvailabilityDto>;

public record CheckUsernameAvailabilityDto(
    bool IsAvailable,
    string? Message
);

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
