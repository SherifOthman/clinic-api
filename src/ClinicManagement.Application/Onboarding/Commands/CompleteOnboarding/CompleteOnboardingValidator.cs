using FluentValidation;

namespace ClinicManagement.Application.Onboarding.Commands;

public class CompleteOnboardingValidator : AbstractValidator<CompleteOnboarding>
{
    public CompleteOnboardingValidator()
    {
        RuleFor(x => x.ClinicName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.SubscriptionPlanId)
            .GreaterThan(0);

        RuleFor(x => x.BranchName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.AddressLine)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.CountryGeoNameId)
            .GreaterThan(0);

        RuleFor(x => x.StateGeoNameId)
            .GreaterThan(0);

        RuleFor(x => x.CityGeoNameId)
            .GreaterThan(0);
    }
}
