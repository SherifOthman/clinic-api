using ClinicManagement.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Auth.Queries;

public record CheckEmailAvailabilityQuery(
    string Email
) : IRequest<CheckEmailAvailabilityDto>;

public record CheckEmailAvailabilityDto(
    bool IsAvailable,
    string? Message
);

public class CheckEmailAvailabilityHandler : IRequestHandler<CheckEmailAvailabilityQuery, CheckEmailAvailabilityDto>
{
    private readonly IApplicationDbContext _context;

    public CheckEmailAvailabilityHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CheckEmailAvailabilityDto> Handle(
        CheckEmailAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return new CheckEmailAvailabilityDto(false, "Email is required");
        }

        var exists = await _context.Users
            .AnyAsync(u => u.Email == request.Email, cancellationToken);
            
        var isAvailable = !exists;

        return new CheckEmailAvailabilityDto(
            isAvailable,
            isAvailable ? null : "Email is already taken"
        );
    }
}
