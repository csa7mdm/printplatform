using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Orders;
using PrintPlatform.Application.Orders.Commands;
using PrintPlatform.Application.Orders.Queries;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/models")]
[Produces("application/json")]
public sealed class ModelsController : ApiControllerBase
{
    private const long MaxUploadBytes = 50L * 1024 * 1024;

    /// <summary>Uploads a 3D model file for analysis.</summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadBytes)]
    [ProducesResponseType(typeof(ModelFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload([FromForm] UploadModelRequest request, CancellationToken ct)
    {
        await using var stream = request.File.OpenReadStream();
        var result = await Mediator.Send(
            new UploadModelFileCommand(
                CurrentUserId,
                stream,
                request.File.FileName,
                request.File.Length,
                request.File.ContentType),
            ct);

        return ToActionResult(result);
    }

    /// <summary>Returns an uploaded model file and its analysis for the authenticated customer.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ModelFileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetModelFileQuery(id, CurrentUserId), ct);
        return ToActionResult(result);
    }
}

public sealed record UploadModelRequest(IFormFile File);
