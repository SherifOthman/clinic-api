using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Commands;

public class SubmitTestimonialHandler : IRequestHandler<SubmitTestimonialCommand, Result>
{
    private readonly ITestimonialRepository _testimonials;
    private readonly IUnitOfWork            _uow;
    private readonly ICurrentUserService    _currentUser;

    public SubmitTestimonialHandler(ITestimonialRepository testimonials, IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _testimonials = testimonials;
        _uow          = uow;
        _currentUser  = currentUser;
    }

    public async Task<Result> Handle(SubmitTestimonialCommand request, CancellationToken ct)
    {
        if (request.Rating is < 1 or > 5)
            return Result.Failure(ErrorCodes.VALIDATION_ERROR, "Rating must be between 1 and 5");

        var clinicId = _currentUser.GetRequiredClinicId();
        var userId   = _currentUser.GetRequiredUserId();

        var existing = await _testimonials.GetByClinicIdAsync(clinicId, ct);
        if (existing is not null)
        {
            existing.Text       = request.Text;
            existing.Rating     = request.Rating;
            existing.IsApproved = false;
            _testimonials.Update(existing);
        }
        else
        {
            await _testimonials.AddAsync(new Testimonial
            {
                ClinicId   = clinicId,
                UserId     = userId,
                Text       = request.Text,
                Rating     = request.Rating,
                IsApproved = false,
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
