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
        var clinicId = _currentUser.GetRequiredClinicId();
        var userId   = _currentUser.GetRequiredUserId();

        var clinic = await _uow.Clinics.GetByIdAsync(clinicId, ct);
        if (clinic is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Clinic not found");

        // One testimonial per clinic — update if exists
        var existing = await _uow.Testimonials.GetByClinicIdAsync(clinicId, ct);
        if (existing is not null)
        {
            existing.AuthorName = request.AuthorName;
            existing.Position   = request.Position;
            existing.Text       = request.Text;
            existing.Rating     = request.Rating;
            existing.AvatarUrl  = request.AvatarUrl;
            existing.IsApproved = true;
            existing.Touch();
            _uow.Testimonials.Update(existing);
        }
        else
        {
            await _uow.Testimonials.AddAsync(new Testimonial
            {
                ClinicId   = clinicId,
                UserId     = userId,
                ClinicName = clinic.Name,
                AuthorName = request.AuthorName,
                Position   = request.Position,
                Text       = request.Text,
                Rating     = request.Rating,
                AvatarUrl  = request.AvatarUrl,
                IsApproved = true,
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
