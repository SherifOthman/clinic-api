using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseasesWithPagination;

public class GetChronicDiseasesWithPaginationQuery : IRequest<Result<PagedResult<ChronicDiseaseDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public bool? IsActive { get; set; }
}
