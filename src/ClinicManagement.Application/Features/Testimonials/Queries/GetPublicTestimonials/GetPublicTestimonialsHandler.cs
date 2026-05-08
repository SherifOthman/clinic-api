using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public class GetPublicTestimonialsHandler : IRequestHandler<GetPublicTestimonialsQuery, Result<List<TestimonialDto>>>
{
    private readonly ITestimonialRepository _testimonials;

    public GetPublicTestimonialsHandler(ITestimonialRepository testimonials) => _testimonials = testimonials;

    public async Task<Result<List<TestimonialDto>>> Handle(GetPublicTestimonialsQuery request, CancellationToken ct)
    {
        var testimonials = await _testimonials.GetApprovedRandomAsync(request.Count, ct);
        return Result.Success(testimonials.Select(TestimonialMapping.ToPublicDto).ToList());
    }
}
