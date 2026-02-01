using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Queries.GetClinics;

public record GetClinicsQuery : IRequest<Result<PagedResult<ClinicDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = false;
    public Guid? SubscriptionPlanId { get; init; }
    public bool? IsActive { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public int? MinUsers { get; init; }
    public int? MaxUsers { get; init; }
}
