using FluentValidation;

namespace ClinicManagement.Application.Features.Clinics.Commands.SetupClinic;

public class SetupClinicCommandValidator : AbstractValidator<SetupClinicCommand>
{
    public SetupClinicCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Clinic name is required")
            .MaximumLength(200)
            .WithMessage("Clinic name must not exceed 200 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .WithMessage("Phone number must not exceed 20 characters");

        RuleFor(x => x.OwnerId)
            .GreaterThan(0)
            .WithMessage("Owner ID is required");

        RuleFor(x => x.SubscriptionPlanId)
            .GreaterThan(0)
            .WithMessage("Subscription plan is required");
    }
}
