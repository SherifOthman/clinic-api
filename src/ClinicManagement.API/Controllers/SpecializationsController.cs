using ClinicManagement.Application.Specializations.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace ClinicManagement.API.Controllers;

[Route("api/specializations")]
public class SpecializationsController : BaseApiController
{
    /// <summary>
    /// Get all specializations
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "ReferenceData")]
    [ProducesResponseType(typeof(IEnumerable<SpecializationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var query = new GetAllSpecializationsQuery();
        var result = await Sender.Send(query, ct);
        return Ok(result);
    }
}
