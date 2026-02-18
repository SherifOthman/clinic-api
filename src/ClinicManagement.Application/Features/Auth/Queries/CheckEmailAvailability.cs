using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public record CheckEmailAvailabilityQuery(
    string Email
) : IRequest<CheckEmailAvailabilityDto>;

public record CheckEmailAvailabilityDto(
    bool IsAvailable,
    string? Message
);

public class CheckEmailAvailabilityHandler : IRequestHandler<CheckEmailAvailabilityQuery, CheckEmailAvailabilityDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CheckEmailAvailabilityHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CheckEmailAvailabilityDto> Handle(
        CheckEmailAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new CheckEmailAvailabilityDto(false, "Email is required");
        }

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
        var isAvailable = user == null;

        return new CheckEmailAvailabilityDto(
            isAvailable,
            isAvailable ? null : "Email is already taken"
        );
    }
}
