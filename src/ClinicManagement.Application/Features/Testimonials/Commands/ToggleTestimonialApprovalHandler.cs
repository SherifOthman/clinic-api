using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Commands;

public class ToggleTestimonialApprovalHandler : IRequestHandler<ToggleTestimonialApprovalCommand, Result>
{
    private readonly IUnitOfWork _uow;
    public ToggleTestimonialApprovalHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(ToggleTestimonialApprovalCommand request, CancellationToken ct)
    {
        var t = await _uow.Testimonials.GetByIdAsync(request.Id, ct);
        if (t is null) return Result.Failure(ErrorCodes.NOT_FOUND, "Testimonial not found");

        t.IsApproved = !t.IsApproved;
        _uow.Testimonials.Update(t);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }
}
