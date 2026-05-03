using FluentValidation;

namespace ClinicManagement.Application.Features.Testimonials.Commands;

public class SubmitTestimonialValidator : AbstractValidator<SubmitTestimonialCommand>
{
    public SubmitTestimonialValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Rating).InclusiveBetween(1, 5);
    }
}
