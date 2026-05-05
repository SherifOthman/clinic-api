using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public class GetPublicTestimonialsHandler : IRequestHandler<GetPublicTestimonialsQuery, Result<List<TestimonialDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetPublicTestimonialsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<TestimonialDto>>> Handle(GetPublicTestimonialsQuery request, CancellationToken ct)
    {
        // Returns a daily-stable random selection of approved testimonials.
        // The seed is today's date — same visitors see the same set all day,
        // but the selection rotates every day automatically.
        var testimonials = await _uow.Testimonials.GetApprovedRandomAsync(request.Count, ct);

        return Result.Success(testimonials.Select(TestimonialMapping.ToPublicDto).ToList());
    }
}
