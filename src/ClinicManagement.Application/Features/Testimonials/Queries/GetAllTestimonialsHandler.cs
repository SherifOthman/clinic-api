using ClinicManagement.Application.Abstractions.Data;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public class GetAllTestimonialsHandler : IRequestHandler<GetAllTestimonialsQuery, List<AdminTestimonialDto>>
{
    private readonly IUnitOfWork _uow;
    public GetAllTestimonialsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<List<AdminTestimonialDto>> Handle(GetAllTestimonialsQuery request, CancellationToken ct)
    {
        var list = await _uow.Testimonials.GetAllAsync(ct);
        return list.Select(t => new AdminTestimonialDto(
            t.Id, t.AuthorName, t.Position, t.ClinicName,
            t.Text, t.Rating, t.AvatarUrl, t.IsApproved, t.CreatedAt
        )).ToList();
    }
}
