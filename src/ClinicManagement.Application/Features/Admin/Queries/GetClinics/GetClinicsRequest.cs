namespace ClinicManagement.Application.Features.Admin.Queries.GetClinics;

public class GetClinicsRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public int? SubscriptionPlanId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int? MinUsers { get; set; }
    public int? MaxUsers { get; set; }
}
