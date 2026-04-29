using ClinicManagement.Application.Abstractions.Data;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public class GetPublicTestimonialsHandler : IRequestHandler<GetPublicTestimonialsQuery, List<TestimonialDto>>
{
    private readonly IUnitOfWork _uow;

    public GetPublicTestimonialsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<TestimonialDto>> Handle(GetPublicTestimonialsQuery request, CancellationToken ct)
    {
        var testimonials = await _uow.Testimonials.GetApprovedAsync(ct);

        return testimonials.Select(t => new TestimonialDto(
            t.AuthorName,
            t.Position,
            t.ClinicName,
            t.Text,
            t.Rating,
            t.AvatarUrl
        )).ToList();
    }
}
