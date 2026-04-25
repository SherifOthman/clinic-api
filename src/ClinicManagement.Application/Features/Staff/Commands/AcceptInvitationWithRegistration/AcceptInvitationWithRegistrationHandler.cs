using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class AcceptInvitationWithRegistrationHandler : IRequestHandler<AcceptInvitationWithRegistrationCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly UserManager<User> _userManager;

    public AcceptInvitationWithRegistrationHandler(IUnitOfWork uow, UserManager<User> userManager)
    {
        _uow = uow;
        _userManager = userManager;
    }

    public async Task<Result> Handle(AcceptInvitationWithRegistrationCommand request, CancellationToken cancellationToken)
    {
        var invitation = await _uow.Invitations.GetByTokenAsync(request.Token, cancellationToken);
        if (invitation is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Invitation not found");

        var gender = Enum.TryParse<Gender>(request.Gender, out var g) ? g : Gender.Male;
        var person = new Person { FullName = request.FullName, Gender = gender };

        // Person must be persisted before UserManager.CreateAsync so the FK is satisfied
        await _uow.Persons.AddAsync(person);
        await _uow.SaveChangesAsync(cancellationToken);

        var user = new User
        {
            UserName = request.UserName,
            Email = invitation.Email,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true,
            PersonId = person.Id,
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return Result.Failure(ErrorCodes.OPERATION_FAILED, string.Join(", ", createResult.Errors.Select(e => e.Description)));

        var roleResult = await _userManager.AddToRoleAsync(user, invitation.Role.ToString());
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Result.Failure(ErrorCodes.OPERATION_FAILED, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        var acceptResult = invitation.Accept(user.Id, DateTimeOffset.UtcNow);
        if (acceptResult.IsFailure) { await _userManager.DeleteAsync(user); return acceptResult; }

        var member = new ClinicMember
        {
            PersonId = person.Id,
            UserId = user.Id,
            ClinicId = invitation.ClinicId,
            Role = invitation.Role,
            IsActive = true,
        };
        await _uow.Members.AddAsync(member);

        if (invitation.Role == ClinicMemberRole.Doctor)
        {
            await _uow.DoctorInfos.AddAsync(new DoctorInfo
            {
                ClinicMemberId = member.Id,
                SpecializationId = invitation.SpecializationId,
            });
        }

        await _uow.Permissions.SeedDefaultsAsync(member.Id, invitation.Role, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
