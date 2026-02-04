using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ISimpleSeedService _seedService;
    private readonly IDatabaseInitializationService _databaseInitializer;
    private readonly ILogger<TestController> _logger;

    public TestController(
        ISimpleSeedService seedService,
        IDatabaseInitializationService databaseInitializer,
        ILogger<TestController> logger)
    {
        _seedService = seedService;
        _databaseInitializer = databaseInitializer;
        _logger = logger;
    }

    [HttpPost("seed-data")]
    public async Task<IActionResult> SeedData()
    {
        try
        {
            _logger.LogInformation("Starting manual seed data test...");
            await _databaseInitializer.InitializeAsync();
            _logger.LogInformation("Seed data test completed successfully");
            return Ok(new { message = "Seed data created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seed data test failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("seed-basic")]
    public async Task<IActionResult> SeedBasicData()
    {
        try
        {
            _logger.LogInformation("Starting basic seed data test...");
            await _seedService.SeedBasicDataAsync();
            _logger.LogInformation("Basic seed data test completed successfully");
            return Ok(new { message = "Basic seed data created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Basic seed data test failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
    }
}