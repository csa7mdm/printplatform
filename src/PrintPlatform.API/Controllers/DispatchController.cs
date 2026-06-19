using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Application.Dispatch;
using PrintPlatform.Application.Dispatch.Commands;
using PrintPlatform.Application.Dispatch.Queries;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/dispatch")]
[Produces("application/json")]
public class DispatchController : ApiControllerBase
{
    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(IReadOnlyList<JobAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetPendingDispatchQuery(), ct);
        return ToActionResult(result);
    }
    
    [HttpGet("my-jobs")]
    [Authorize(Roles = "PrinterOwner")]
    [ProducesResponseType(typeof(IReadOnlyList<JobAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyJobs(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetMyJobsQuery(CurrentUserId), ct);
        return ToActionResult(result);
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(typeof(JobAssignmentDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AssignJob([FromBody] AssignJobRequest request, CancellationToken ct)
    {
        var command = new AssignJobCommand(request.OrderItemId, request.PrinterId, request.PrinterOwnerUserId, request.PayoutAmount, request.OperatorNotes);
        var result = await Mediator.Send(command, ct);
        return ToActionResult(result);
    }

    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "PrinterOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AcceptJob(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new AcceptJobCommand(id, CurrentUserId), ct);
        return ToActionResult(result);
    }
    
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "PrinterOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectJob(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new RejectJobCommand(id, CurrentUserId), ct);
        return ToActionResult(result);
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = "PrinterOwner")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> StartPrinting(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new StartPrintingCommand(id, CurrentUserId), ct);
        return ToActionResult(result);
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "PrinterOwner")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteJob(
        Guid id,
        [FromForm] UploadCompletionPhotosRequest request,
        [FromServices] IFileStorageService storage,
        CancellationToken ct)
    {
        if (request.Photos is null || request.Photos.Length < 3)
        {
            return Problem(Error.Validation(
                "Dispatch.CompletionPhotos",
                "At least 3 completion photos are required."));
        }

        var photoKeys = new List<string>(request.Photos.Length);
        foreach (var photo in request.Photos)
        {
            var extension = Path.GetExtension(photo.FileName);
            var objectKey = $"dispatch/completion-photos/{CurrentUserId}/{id}/{Guid.NewGuid():N}{extension}";

            await using var stream = photo.OpenReadStream();
            var photoKey = await storage.UploadAsync(
                stream,
                objectKey,
                string.IsNullOrWhiteSpace(photo.ContentType) ? "application/octet-stream" : photo.ContentType,
                cancellationToken: ct);

            photoKeys.Add(photoKey);
        }

        var result = await Mediator.Send(new UploadCompletionPhotosCommand(id, CurrentUserId, photoKeys), ct);
        return ToActionResult(result);
    }

    [HttpPost("{id:guid}/qc/approve")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveQc(
        Guid id,
        [FromBody] ApproveQcRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new ApproveQcCommand(
                id,
                CurrentUserId,
                request.Notes ?? string.Empty,
                request.Photos ?? [],
                request.Checklist ?? string.Empty),
            ct);

        return ToActionResult(result);
    }

    [HttpPost("{id:guid}/qc/reject")]
    [Authorize(Roles = "Admin,Operator")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectQc(
        Guid id,
        [FromBody] RejectQcRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new RejectQcCommand(
                id,
                CurrentUserId,
                request.Notes ?? string.Empty,
                request.Photos ?? [],
                request.Checklist ?? string.Empty,
                request.NeedsReprint),
            ct);

        return ToActionResult(result);
    }
}

public record AssignJobRequest(Guid OrderItemId, Guid PrinterId, Guid PrinterOwnerUserId, decimal PayoutAmount, string? OperatorNotes);

public sealed record UploadCompletionPhotosRequest(IFormFile[]? Photos);

public sealed record ApproveQcRequest(string? Notes, List<string>? Photos, string? Checklist);

public sealed record RejectQcRequest(string? Notes, List<string>? Photos, string? Checklist, bool NeedsReprint);
