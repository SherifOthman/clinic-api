using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public class GetMyTestimonialHandler : IRequestHandler<GetMyTestimonialQuery, Result<MyTestimonialDto?>>
{
    private readonly ITestimonialRepository _testimonials;
    private readonly ICurrentUserService    _currentUser;

    public GetMyTestimonialHandler(ITestimonialRepository testimonials, ICurrentUserService currentUser)
    {
        _testimonials = testimonials;
        _currentUser  = currentUser;
    }

    public async Task<Result<MyTestimonialDto?>> Handle(GetMyTestimonialQuery request, CancellationToken ct)
    {
        var clinicId = _currentUser.GetRequiredClinicId();
        var t = await _testimonials.GetByClinicIdAsync(clinicId, ct);

        if (t is null)
            return Result.Success<MyTestimonialDto?>(null);

        return Result.Success<MyTestimonialDto?>(TestimonialMapping.ToMyDto(t));
    }
}
