using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.MedicalSupplies.Commands.CreateMedicalSupply;
using ClinicManagement.Application.Features.MedicalSupplies.Queries.GetMedicalSupplies;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class MedicalSuppliesController : BaseApiController
{
    private readonly IMediator _mediator;

    public MedicalSuppliesController(IMediator mediator) { _mediator = mediator;
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetMedicalSupplies(
        Guid branchId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] bool? isLowStock = null)
    {
        var paginationRequest = new SearchablePaginationRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        if (isLowStock.HasValue)
        {
            paginationRequest.Filters.Add("isLowStock", isLowStock.Value);
        }

        var result = await _mediator.Send(new GetMedicalSuppliesQuery(branchId, paginationRequest));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicalSupply(CreateMedicalSupplyCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleCreateResult(result, nameof(GetMedicalSupplies), new { branchId = command.ClinicBranchId });
    }
}
