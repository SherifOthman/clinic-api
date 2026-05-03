using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public class GetMyTestimonialHandler : IRequestHandler<GetMyTestimonialQuery, Result<MyTestimonialDto?>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetMyTestimonialHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<MyTestimonialDto?>> Handle(GetMyTestimonialQuery request, CancellationToken ct)
    {
        var clinicId = _currentUser.GetRequiredClinicId();
        var t = await _uow.Testimonials.GetByClinicIdAsync(clinicId, ct);

        if (t is null)
            return Result.Success<MyTestimonialDto?>(null);

        // AuthorName, Position, AvatarUrl, ClinicName come from navigation properties —
        // always the current values, no stale denormalized data on the entity.
        return Result.Success<MyTestimonialDto?>(new MyTestimonialDto(
            AuthorName: t.User.FullName,
            Position:   "Clinic Owner",
            Text:       t.Text,
            Rating:     t.Rating,
            AvatarUrl:  t.User.ProfileImageUrl,
            IsApproved: t.IsApproved));
    }
}
