using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Common.Models;

public class PatientSearchRequest : PaginationRequest
{
    public Gender? Gender { get; set; }
    public DateTime? DateOfBirthFrom { get; set; }
    public DateTime? DateOfBirthTo { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    public PatientSearchRequest()
    {
    }

    public PatientSearchRequest(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
        : base(pageNumber, pageSize)
    {
        SearchTerm = searchTerm;
        SortBy = sortBy;
        SortDescending = sortDescending;
    }

    public bool HasGenderFilter => Gender.HasValue;
    public bool HasDateOfBirthFilter => DateOfBirthFrom.HasValue || DateOfBirthTo.HasValue;
    public bool HasCreatedDateFilter => CreatedFrom.HasValue || CreatedTo.HasValue;
    public bool HasAgeFilter => MinAge.HasValue || MaxAge.HasValue;
    public bool HasSearchTerm => !string.IsNullOrWhiteSpace(SearchTerm);
}