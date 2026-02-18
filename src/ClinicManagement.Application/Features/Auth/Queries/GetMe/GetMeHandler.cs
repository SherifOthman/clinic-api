using ClinicManagement.Domain.Repositories;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

public class GetMeHandler : IRequestHandler<GetMeQuery, GetMeDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetMeHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetMeDto?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            return null;

        var roles = await _unitOfWork.Users.GetUserRolesAsync(user.Id, cancellationToken);

        // Check if user has completed onboarding by checking if they own a clinic
        var hasClinic = await _unitOfWork.Users.HasCompletedClinicOnboardingAsync(user.Id, cancellationToken);

        return new GetMeDto(
            user.Id,
            user.UserName!,
            user.FirstName,
            user.LastName,
            user.Email!,
            user.PhoneNumber,
            user.ProfileImageUrl,
            roles.ToList(),
            user.EmailConfirmed,
            hasClinic
        );
    }
}
