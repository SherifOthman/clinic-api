using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize(Roles = RoleNames.SystemAdmin)]
public class UserAnalyticsController : BaseApiController
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserAnalyticsController(IMediator mediator, IDateTimeProvider dateTimeProvider) : base(mediator)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(UserAnalyticsDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardData(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.UtcNow;
        
        // For now, return mock data since we don't have user session tracking implemented
        var dashboardData = new UserAnalyticsDashboardDto
        {
            TotalSessions = 1250,
            ActiveSessions = 45,
            UniqueUsers = 320,
            SessionsByCountry = new Dictionary<string, int>
            {
                { "United States", 450 },
                { "Canada", 200 },
                { "United Kingdom", 150 },
                { "Egypt", 180 },
                { "Germany", 120 },
                { "France", 100 },
                { "Others", 50 }
            },
            SessionsByCity = new Dictionary<string, int>
            {
                { "New York", 180 },
                { "Toronto", 120 },
                { "London", 100 },
                { "Cairo", 150 },
                { "Berlin", 80 },
                { "Paris", 70 },
                { "Others", 550 }
            },
            SessionsByDay = new Dictionary<string, int>
            {
                { now.AddDays(-6).ToString("yyyy-MM-dd"), 45 },
                { now.AddDays(-5).ToString("yyyy-MM-dd"), 52 },
                { now.AddDays(-4).ToString("yyyy-MM-dd"), 38 },
                { now.AddDays(-3).ToString("yyyy-MM-dd"), 65 },
                { now.AddDays(-2).ToString("yyyy-MM-dd"), 48 },
                { now.AddDays(-1).ToString("yyyy-MM-dd"), 55 },
                { now.ToString("yyyy-MM-dd"), 42 }
            },
            TopCountries = new List<TopCountryDto>
            {
                new() { Country = "United States", SessionCount = 450, UniqueUsers = 120, Percentage = 36.0m },
                new() { Country = "Egypt", SessionCount = 180, UniqueUsers = 65, Percentage = 14.4m },
                new() { Country = "Canada", SessionCount = 200, UniqueUsers = 55, Percentage = 16.0m },
                new() { Country = "United Kingdom", SessionCount = 150, UniqueUsers = 40, Percentage = 12.0m },
                new() { Country = "Germany", SessionCount = 120, UniqueUsers = 25, Percentage = 9.6m }
            }
        };

        return Ok(dashboardData);
    }
}

public class UserAnalyticsDashboardDto
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public int UniqueUsers { get; set; }
    public Dictionary<string, int> SessionsByCountry { get; set; } = new();
    public Dictionary<string, int> SessionsByCity { get; set; } = new();
    public Dictionary<string, int> SessionsByDay { get; set; } = new();
    public List<TopCountryDto> TopCountries { get; set; } = new();
}

public class TopCountryDto
{
    public string Country { get; set; } = string.Empty;
    public int SessionCount { get; set; }
    public int UniqueUsers { get; set; }
    public decimal Percentage { get; set; }
}