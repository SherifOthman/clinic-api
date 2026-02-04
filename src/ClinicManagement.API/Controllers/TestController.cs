using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IDatabaseInitializationService _databaseInitializer;
    private readonly ILogger<TestController> _logger;

    public TestController(
        IDatabaseInitializationService databaseInitializer,
        ILogger<TestController> logger)
    {
        _databaseInitializer = databaseInitializer;
        _logger = logger;
    }

    [HttpPost("seed-data")]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            _logger.LogInformation("Starting comprehensive seed data test...");
            await _databaseInitializer.InitializeAsync();
            _logger.LogInformation("Comprehensive seed data test completed successfully");
            return Ok(new { message = "Comprehensive seed data created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seed data test failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }
}