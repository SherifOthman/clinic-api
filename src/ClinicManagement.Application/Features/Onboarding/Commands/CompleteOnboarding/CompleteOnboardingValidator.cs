using FluentValidation;

namespace ClinicManagement.Application.Features.Onboarding.Commands;

public class CompleteOnboardingValidator : AbstractValidator<CompleteOnboarding>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.ClinicName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.SubscriptionPlanId)
            .NotEmpty();

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AddressLine)
            .NotEmpty()
            .MaximumLength(500);
    }
}
