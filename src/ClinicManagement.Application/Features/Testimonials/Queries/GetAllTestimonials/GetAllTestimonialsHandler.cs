using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public class GetAllTestimonialsHandler
    : IRequestHandler<GetAllTestimonialsQuery, Result<PaginatedResult<AdminTestimonialDto>>>
{
    private readonly ITestimonialRepository _testimonials;

    public GetAllTestimonialsHandler(ITestimonialRepository testimonials) => _testimonials = testimonials;

    public async Task<Result<PaginatedResult<AdminTestimonialDto>>> Handle(
        GetAllTestimonialsQuery request, CancellationToken ct)
    {
        var (items, total) = await _testimonials.GetPagedAsync(request.PageNumber, request.PageSize, ct);
        var dtos = items.Select(TestimonialMapping.ToAdminDto).ToList();
        return Result.Success(PaginatedResult<AdminTestimonialDto>.Create(dtos, total, request.PageNumber, request.PageSize));
    }
}
