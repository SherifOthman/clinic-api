using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Testimonials.Queries;

public class GetAllTestimonialsHandler
    : IRequestHandler<GetAllTestimonialsQuery, Result<PaginatedResult<AdminTestimonialDto>>>
{
    private readonly IUnitOfWork _uow;
    public GetAllTestimonialsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<PaginatedResult<AdminTestimonialDto>>> Handle(
        GetAllTestimonialsQuery request, CancellationToken ct)
    {
        var (items, total) = await _uow.Testimonials.GetPagedAsync(
            request.PageNumber, request.PageSize, ct);

        var dtos = items.Select(TestimonialMapping.ToAdminDto).ToList();

        return Result.Success(
            PaginatedResult<AdminTestimonialDto>.Create(dtos, total, request.PageNumber, request.PageSize));
    }
}
