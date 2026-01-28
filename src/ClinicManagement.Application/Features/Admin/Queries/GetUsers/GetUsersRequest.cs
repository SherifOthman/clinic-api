namespace ClinicManagement.Application.Features.Admin.Queries.GetUsers;

public class GetUsersRequest
{

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? SearchTerm { get; set; }

    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = false;

    public bool? EmailConfirmed { get; set; }

    public string? Role { get; set; }

    public int? ClinicId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }
}
