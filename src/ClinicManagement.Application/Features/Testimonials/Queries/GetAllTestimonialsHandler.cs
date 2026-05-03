using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public class GetAllTestimonialsHandler : IRequestHandler<GetAllTestimonialsQuery, Result<List<AdminTestimonialDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetAllTestimonialsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<AdminTestimonialDto>>> Handle(GetAllTestimonialsQuery request, CancellationToken ct)
    {
        var list = await _uow.Testimonials.GetAllAsync(ct);

        var dtos = list.Select(t => new AdminTestimonialDto(
            Id:         t.Id,
            AuthorName: t.User.FullName,
            Position:   "Clinic Owner",
            ClinicName: t.Clinic.Name,
            Text:       t.Text,
            Rating:     t.Rating,
            AvatarUrl:  t.User.ProfileImageUrl,
            IsApproved: t.IsApproved,
            CreatedAt:  t.CreatedAt
        )).ToList();

        return Result.Success(dtos);
    }
}
