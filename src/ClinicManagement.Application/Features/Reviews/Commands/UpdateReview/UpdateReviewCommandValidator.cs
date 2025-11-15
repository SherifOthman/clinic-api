using FluentValidation;

namespace ClinicManagement.Application.Features.Reviews.Commands.UpdateReview;

public class UpdateReviewCommandValidator : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Review ID is required");

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
