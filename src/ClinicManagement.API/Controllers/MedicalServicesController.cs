using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.MedicalServices.Commands.CreateMedicalService;
using ClinicManagement.Application.Features.MedicalServices.Queries.GetMedicalServices;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class MedicalServicesController : BaseApiController
{
    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetMedicalServices(Guid branchId)
    {
        var result = await Mediator.Send(new GetMedicalServicesQuery(branchId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicalService(CreateMedicalServiceCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}