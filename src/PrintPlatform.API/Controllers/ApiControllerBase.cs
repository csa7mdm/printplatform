using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Domain.Shared;

namespace PrintPlatform.API.Controllers;

/// <summary>
/// Base controller providing MediatR access and a uniform translation of
/// <see cref="Result{T}"/> into HTTP responses.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>The authenticated user's ID, taken from the JWT 'sub' or 'nameidentifier' claim.</summary>
    protected Guid CurrentUserId
    {
        get
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }
    
    /// <summary>The authenticated printer-owner profile id, taken from the JWT.</summary>
    protected Guid CurrentProfileId
    {
        get
        {
            var raw = User.FindFirstValue("profile_id")
                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? User.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
        }
    }

    protected IActionResult ToActionResult<T>(Result<T> result) =>
        result.IsSuccess
            ? Ok(result.Value)
            : Problem(result.Error);

    protected IActionResult ToActionResult(Result result) =>
        result.IsSuccess
            ? Ok()
            : Problem(result.Error);

    protected IActionResult Problem(Error error)
    {
        var status = error.Type switch
        {
            ErrorType.NotFound     => StatusCodes.Status404NotFound,
            ErrorType.Validation   => StatusCodes.Status400BadRequest,
            ErrorType.Conflict     => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            _                      => StatusCodes.Status500InternalServerError,
        };

        return Problem(
            statusCode: status,
            title: error.Code,
            detail: error.Message);
    }
}
