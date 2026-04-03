using ClinicManagement.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Auth.Queries;

public record CheckUsernameAvailabilityQuery(
    string Username
) : IRequest<CheckUsernameAvailabilityDto>;

public record CheckUsernameAvailabilityDto(
    bool IsAvailable,
    string? Message
);

public class CheckUsernameAvailabilityHandler : IRequestHandler<CheckUsernameAvailabilityQuery, CheckUsernameAvailabilityDto>
{
    private readonly IApplicationDbContext _context;

    public CheckUsernameAvailabilityHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CheckUsernameAvailabilityDto> Handle(
        CheckUsernameAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new CheckUsernameAvailabilityDto(false, "Username is required");
        }

        var exists = await _context.Users
            .AnyAsync(u => u.UserName == request.Username, cancellationToken);
            
        var isAvailable = !exists;

        return new CheckUsernameAvailabilityDto(
            isAvailable,
            isAvailable ? null : "Username is already taken"
        );
    }
}
