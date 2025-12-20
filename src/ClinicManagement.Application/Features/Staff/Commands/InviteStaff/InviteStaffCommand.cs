using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Staff.Commands.InviteStaff;

public record InviteStaffCommand : IRequest<Result>
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public UserRole Role { get; init; } // Doctor or Receptionist
    public int? SpecializationId { get; init; } // Required for doctors
    public string? PhoneNumber { get; init; }
    
    // ClinicId and InvitedByUserId are injected from ICurrentUserService - not from client
}
