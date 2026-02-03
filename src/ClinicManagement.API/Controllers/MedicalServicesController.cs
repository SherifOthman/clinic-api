using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.MedicalServices.Commands.CreateMedicalService;
using ClinicManagement.Application.Features.MedicalServices.Queries.GetMedicalServices;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class MedicalServicesController : BaseApiController
{
    public MedicalServicesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetMedicalServices(Guid branchId)
    {
        var result = await Mediator.Send(new GetMedicalServicesQuery(branchId));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicalService(CreateMedicalServiceCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreateResult(result, nameof(GetMedicalServices), new { branchId = command.ClinicBranchId });
    }
}
