using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Marketplace;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/materials")]
[Produces("application/json")]
public sealed class MaterialsController : ApiControllerBase
{
    /// <summary>Lists material options. By default only active ones are returned.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<MaterialOptionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetMaterialsQuery(activeOnly), ct);
        return ToActionResult(result);
    }

    /// <summary>Gets a single material option by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MaterialOptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMaterialByIdQuery(id), ct);
        return ToActionResult(result);
    }
}
