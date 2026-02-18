using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

public class GetMeHandler : IRequestHandler<GetMeQuery, GetMeDto?>
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public GetMeHandler(
        UserManager<User> userManager,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<GetMeDto?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

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
