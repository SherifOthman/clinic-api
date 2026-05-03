using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Commands;

public class SubmitTestimonialHandler : IRequestHandler<SubmitTestimonialCommand, Result>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public SubmitTestimonialHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(SubmitTestimonialCommand request, CancellationToken ct)
    {
        if (request.Rating is < 1 or > 5)
            return Result.Failure(ErrorCodes.VALIDATION_ERROR, "Rating must be between 1 and 5");

        var clinicId = _currentUser.GetRequiredClinicId();
        var userId   = _currentUser.GetRequiredUserId();

        // Resolve author info from source of truth — not from request body
        var user = await _uow.Users.GetByIdAsync(userId, ct);
        if (user is null)
            return Result.Failure(ErrorCodes.USER_NOT_FOUND, "User not found");

        var clinic = await _uow.Clinics.GetByIdAsync(clinicId, ct);
        if (clinic is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Clinic not found");

        // One testimonial per clinic — update if exists, create if not
        var existing = await _uow.Testimonials.GetByClinicIdAsync(clinicId, ct);
        if (existing is not null)
        {
            existing.AuthorName = user.FullName;
            existing.Position   = "Clinic Owner";
            existing.ClinicName = clinic.Name;
            existing.AvatarUrl  = user.ProfileImageUrl;
            existing.Text       = request.Text;
            existing.Rating     = request.Rating;
            existing.IsApproved = false; // re-submit resets approval — admin must re-approve
            existing.Touch();
            _uow.Testimonials.Update(existing);
        }
        else
        {
            await _uow.Testimonials.AddAsync(new Testimonial
            {
                ClinicId   = clinicId,
                UserId     = userId,
                AuthorName = user.FullName,
                Position   = "Clinic Owner",
                ClinicName = clinic.Name,
                AvatarUrl  = user.ProfileImageUrl,
                Text       = request.Text,
                Rating     = request.Rating,
                IsApproved = false,
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
