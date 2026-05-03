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

        // Read live name and avatar from User — stays current if profile is updated
        var userId = _currentUser.GetRequiredUserId();
        var user   = await _uow.Users.GetByIdAsync(userId, ct);

        return Result.Success<MyTestimonialDto?>(new MyTestimonialDto(
            AuthorName: user?.FullName ?? t.AuthorName,
            Position:   "Clinic Owner",
            Text:       t.Text,
            Rating:     t.Rating,
            AvatarUrl:  user?.ProfileImageUrl ?? t.AvatarUrl,
            IsApproved: t.IsApproved));
    }
}
