using FluentValidation;

namespace ClinicManagement.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User ID is required");

        RuleFor(x => x.ClinicId)
            .GreaterThan(0)
            .WithMessage("Clinic ID is required");

        RuleFor(x => x.Quote)
            .NotEmpty()
            .WithMessage("Review quote is required")
            .MaximumLength(1000)
            .WithMessage("Quote cannot exceed 1000 characters");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");
    }
}
