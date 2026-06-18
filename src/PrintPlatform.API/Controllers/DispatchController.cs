using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    
    // Other endpoints like /start, /complete, /qc/approve would follow the same pattern
}

public record AssignJobRequest(Guid OrderItemId, Guid PrinterId, Guid PrinterOwnerUserId, decimal PayoutAmount, string? OperatorNotes);
