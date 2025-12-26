using FluentValidation;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

internal class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
{
    public CompleteOnboardingCommandValidator()
    {
        RuleFor(x => x.ClinicName)
            .NotEmpty()
            .WithMessage("Clinic name is required")
            .MaximumLength(100)
            .WithMessage("Clinic name cannot exceed 100 characters");

        RuleFor(x => x.ClinicPhone)
            .MaximumLength(20)
            .WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.ClinicPhone));

        RuleFor(x => x.SubscriptionPlanId)
            .GreaterThan(0)
            .WithMessage("Please select a valid subscription plan");
    }
}