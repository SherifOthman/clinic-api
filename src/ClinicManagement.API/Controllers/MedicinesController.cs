using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;
using ClinicManagement.Application.Features.Medicines.Commands.DeleteMedicine;
using ClinicManagement.Application.Features.Medicines.Commands.UpdateMedicine;
using ClinicManagement.Application.Features.Medicines.Queries.GetMedicine;
using ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class MedicinesController : BaseApiController
{
    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetMedicines(Guid branchId)
    {
        var result = await Mediator.Send(new GetMedicinesQuery(branchId));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicine(Guid id)
    {
        var result = await Mediator.Send(new GetMedicineQuery(id));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicine(CreateMedicineCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicine(Guid id, UpdateMedicineCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicine(Guid id)
    {
        var result = await Mediator.Send(new DeleteMedicineCommand(id));
        return Ok(result);
    }
}