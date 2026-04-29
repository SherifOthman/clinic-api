using FluentValidation;

namespace ClinicManagement.Application.Features.Testimonials.Commands;

public class SubmitTestimonialValidator : AbstractValidator<SubmitTestimonialCommand>
{
    public SubmitTestimonialValidator()
    {
        RuleFor(x => x.AuthorName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
        RuleFor(x => x.AvatarUrl).MaximumLength(500).When(x => x.AvatarUrl != null);
    }
}
