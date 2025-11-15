using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Reviews.Commands.CreateReview;
using ClinicManagement.Application.Features.Reviews.Commands.DeleteReview;
using ClinicManagement.Application.Features.Reviews.Commands.UpdateReview;
using ClinicManagement.Application.Features.Reviews.Queries.GetReviewById;
using ClinicManagement.Application.Features.Reviews.Queries.GetReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReviewsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReviews(CancellationToken cancellationToken)
    {
        var query = new GetReviewsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return result.Success
            ? Ok(result.Value)
            : BadRequest(result.ToApiError());
    }

    [HttpGet("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReviewById(int id, CancellationToken cancellationToken)
    {
        var query = new GetReviewByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        return result.Success
            ? Ok(result.Value)
            : NotFound(result.ToApiError());
    }

    [Authorize]
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReview(CreateReviewCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.Success
            ? CreatedAtAction(nameof(GetReviewById), new { id = result.Value!.Id }, result.Value)
            : BadRequest(result.ToApiError());
    }

    [Authorize]
    [HttpPut("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReview(int id, UpdateReviewCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await _mediator.Send(command, cancellationToken);

        return result.Success
            ? Ok(result.Value)
            : NotFound(result.ToApiError());
    }

    [Authorize]
    [HttpDelete("{id}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteReviewCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);

        return result.Success
            ? NoContent()
            : NotFound(result.ToApiError());
    }
}
