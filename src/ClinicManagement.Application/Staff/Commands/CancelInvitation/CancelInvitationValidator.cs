using FluentValidation;

namespace ClinicManagement.Application.Staff.Commands;

public class CancelInvitationValidator : AbstractValidator<CancelInvitationCommand>
{
    public CancelInvitationValidator()
    {
        RuleFor(x => x.InvitationId)
            .NotEmpty()
            .WithMessage("Invitation ID is required");
    }
}
