using ClinicManagement.API.Extensions;
using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Admin.Commands.DeleteUser;
using ClinicManagement.Application.Features.Admin.Commands.ToggleUserStatus;
using ClinicManagement.Application.Features.Admin.Commands.UpdateUser;
using ClinicManagement.Application.Features.Admin.Queries.GetUser;
using ClinicManagement.Application.Features.Admin.Queries.GetUsers;
using ClinicManagement.Application.Features.Admin.Queries.GetClinics;
using ClinicManagement.Application.Features.Admin.Queries.GetClinic;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/admin")]
[Authorize(Roles = RoleNames.SystemAdmin + "," + RoleNames.ClinicOwner)]
public class AdminController : BaseApiController
{
    public AdminController(IMediator mediator) : base(mediator) { }

    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResult<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        var query = new GetUsersQuery(request);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("users/{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetUserQuery { Id = id };
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPut("users/{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(MessageCodes.Controller.ID_MISMATCH);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("users/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand { Id = id };
        var result = await Mediator.Send(command, cancellationToken);
        return HandleDeleteResult(result);
    }

    [HttpPost("users/{id}/toggle-status")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleUserStatus(Guid id, CancellationToken cancellationToken)
    {
        var command = new ToggleUserStatusCommand { Id = id };
        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    #region Clinic Management

    [HttpGet("clinics")]
    [ProducesResponseType(typeof(PagedResult<ClinicDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClinics([FromQuery] GetClinicsQuery query, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("clinics/{id}")]
    [ProducesResponseType(typeof(ClinicDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClinic(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetClinicQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    #endregion
}