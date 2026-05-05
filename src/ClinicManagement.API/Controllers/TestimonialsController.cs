using ClinicManagement.API.Authorization;
using ClinicManagement.API.RateLimiting;
using ClinicManagement.Application.Features.Testimonials.Commands;
using ClinicManagement.Application.Features.Testimonials.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[Route("api/testimonials")]
public class TestimonialsController : BaseApiController
{
    /// <summary>Public — approved testimonials for the marketing website.</summary>
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.AnonStatic)]
    [ProducesResponseType(typeof(List<TestimonialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic([FromQuery] int count = 3, CancellationToken ct = default)
    {
        var result = await Sender.Send(new GetPublicTestimonialsQuery(count), ct);
        return HandleResult(result, "Failed to retrieve testimonials");
    }

    /// <summary>SuperAdmin — all testimonials with approval status.</summary>
    [HttpGet("all")]
    [Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(List<AdminTestimonialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await Sender.Send(new GetAllTestimonialsQuery(), ct);
        return HandleResult(result, "Failed to retrieve testimonials");
    }

    /// <summary>SuperAdmin — toggle published/hidden for a testimonial.</summary>
    [HttpPatch("{id:guid}/toggle")]
    [Authorize(Policy = AuthorizationPolicies.SuperAdmin)]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Toggle(Guid id, CancellationToken ct)
    {
        var result = await Sender.Send(new ToggleTestimonialApprovalCommand(id), ct);
        return HandleNoContent(result, "Failed to toggle testimonial");
    }

    /// <summary>ClinicOwner — get their own testimonial.</summary>
    [HttpGet("mine")]
    [Authorize(Policy = AuthorizationPolicies.ClinicOwner)]
    [EnableRateLimiting(RateLimitPolicies.UserReads)]
    [ProducesResponseType(typeof(MyTestimonialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var result = await Sender.Send(new GetMyTestimonialQuery(), ct);
        if (result.IsFailure) return HandleResult(result, "Failed to retrieve testimonial");
        return result.Value is null ? NoContent() : Ok(result.Value);
    }

    /// <summary>ClinicOwner — submit or update their testimonial.</summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ClinicOwner)]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Submit([FromBody] SubmitTestimonialCommand command, CancellationToken ct)
    {
        var result = await Sender.Send(command, ct);
        return HandleNoContent(result, "Failed to submit testimonial");
    }
}
