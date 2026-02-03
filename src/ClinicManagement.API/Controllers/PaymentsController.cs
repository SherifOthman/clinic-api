using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Payments.Commands.CreatePayment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class PaymentsController : BaseApiController
{
    public PaymentsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment(CreatePaymentCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreateResult(result, "GetPayment", new { id = result.Value });
    }
}
