using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Commands;

public class ToggleTestimonialApprovalHandler : IRequestHandler<ToggleTestimonialApprovalCommand, Result>
{
    private readonly ITestimonialRepository _testimonials;
    private readonly IUnitOfWork            _uow;

    public ToggleTestimonialApprovalHandler(ITestimonialRepository testimonials, IUnitOfWork uow)
    {
        _testimonials = testimonials;
        _uow          = uow;
    }

    public async Task<Result> Handle(ToggleTestimonialApprovalCommand request, CancellationToken ct)
    {
        var t = await _testimonials.GetByIdAsync(request.Id, ct);
        if (t is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Testimonial not found");

        t.IsApproved = !t.IsApproved;
        _testimonials.Update(t);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
