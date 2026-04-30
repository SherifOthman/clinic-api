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
        var testimonials = await _uow.Testimonials.GetApprovedAsync(ct);

        var list = testimonials.Select(t => new TestimonialDto(
            t.AuthorName,
            t.Position,
            t.ClinicName,
            t.Text,
            t.Rating,
            t.AvatarUrl
        )).ToList();

        return Result.Success(list);
    }
}
