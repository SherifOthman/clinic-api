using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
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

        return Result.Success<MyTestimonialDto?>(
            new MyTestimonialDto(t.AuthorName, t.Position, t.Text, t.Rating, t.AvatarUrl, t.IsApproved));
    }
}
