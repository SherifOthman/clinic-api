using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.MedicalSupplies.Commands.CreateMedicalSupply;
using ClinicManagement.Application.Features.MedicalSupplies.Queries.GetMedicalSupplies;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class MedicalSuppliesController : BaseApiController
{
    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetMedicalSupplies(Guid branchId)
    {
        var result = await Mediator.Send(new GetMedicalSuppliesQuery(branchId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicalSupply(CreateMedicalSupplyCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}