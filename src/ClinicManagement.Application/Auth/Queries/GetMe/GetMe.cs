using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Auth.Queries;

public record GetMeQuery(Guid UserId) : IRequest<GetMeDto?>;

public record GetMeDto(
    Guid Id,
    string UserName,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string? ProfileImageUrl,
    List<string> Roles,
    bool EmailConfirmed,
    bool OnboardingCompleted
);

public class GetMeHandler : IRequestHandler<GetMeQuery, GetMeDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<Domain.Entities.User> _userManager;

    public GetMeHandler(
        IApplicationDbContext context,
        UserManager<Domain.Entities.User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<GetMeDto?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
            
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        var hasClinic = await _context.Clinics
            .AnyAsync(c => c.OwnerUserId == user.Id, cancellationToken);

        var dto = user.Adapt<GetMeDto>();
        return dto with 
        { 
            Roles = roles.ToList(), 
            OnboardingCompleted = hasClinic 
        };
    }
}
