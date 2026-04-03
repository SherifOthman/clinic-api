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

        RuleFor(x => x.CountryGeoNameId)
            .GreaterThan(0);

        RuleFor(x => x.StateGeoNameId)
            .GreaterThan(0);

        RuleFor(x => x.CityGeoNameId)
            .GreaterThan(0);

        When(x => x.ProvideMedicalServices, () =>
        {
            RuleFor(x => x.SpecializationId)
                .NotEmpty()
                .WithMessage("Specialization is required when providing medical services");
        });
    }
}
