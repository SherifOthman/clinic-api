namespace ClinicManagement.Application.DTOs;

public class ReviewDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public string Quote { get; set; } = string.Empty;
    public int Rating { get; set; }
}

public class CreateReviewDto
{
    public int UserId { get; set; }
    public int ClinicId { get; set; }
    public string Quote { get; set; } = string.Empty;
    public int Rating { get; set; } = 5;
}
