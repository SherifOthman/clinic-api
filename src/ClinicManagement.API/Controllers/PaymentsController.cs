using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Payments.Commands.CreatePayment;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class PaymentsController : BaseApiController
{
    [HttpPost]
    public async Task<IActionResult> CreatePayment(CreatePaymentCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}