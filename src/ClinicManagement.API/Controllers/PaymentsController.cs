using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Payments.Commands.CreatePayment;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class PaymentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator) { _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment(CreatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleCreateResult(result, "GetPayment", new { id = result.Value });
    }
}
