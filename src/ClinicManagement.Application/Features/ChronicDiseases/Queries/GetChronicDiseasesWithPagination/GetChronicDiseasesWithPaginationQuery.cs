using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseasesWithPagination;

public class GetChronicDiseasesWithPaginationQuery : IRequest<Result<PagedResult<ChronicDiseaseDto>>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public bool? IsActive { get; set; }

    public GetChronicDiseasesWithPaginationQuery(int pageNumber, int pageSize, string? searchTerm = null, 
        string? sortBy = null, bool sortDescending = false, bool? isActive = null)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        SearchTerm = searchTerm;
        SortBy = sortBy;
        SortDescending = sortDescending;
        IsActive = isActive;
    }
}
