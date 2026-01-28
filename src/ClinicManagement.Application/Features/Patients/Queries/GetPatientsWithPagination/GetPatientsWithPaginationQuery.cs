using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientsWithPagination;

public class GetPatientsWithPaginationQuery : PaginationRequest, IRequest<Result<PagedResult<PatientDto>>>
{
    public Gender? Gender { get; set; }
    public DateTime? DateOfBirthFrom { get; set; }
    public DateTime? DateOfBirthTo { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }

    public GetPatientsWithPaginationQuery()
    {
    }

    public GetPatientsWithPaginationQuery(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDescending = false, Gender? gender = null, DateTime? dateOfBirthFrom = null, DateTime? dateOfBirthTo = null, DateTime? createdFrom = null, DateTime? createdTo = null, int? minAge = null, int? maxAge = null)
        : base(pageNumber, pageSize)
    {
        SearchTerm = searchTerm;
        SortBy = sortBy;
        SortDescending = sortDescending;
        Gender = gender;
        DateOfBirthFrom = dateOfBirthFrom;
        DateOfBirthTo = dateOfBirthTo;
        CreatedFrom = createdFrom;
        CreatedTo = createdTo;
        MinAge = minAge;
        MaxAge = maxAge;
    }
}
