using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintPlatform.Application.Identity.Commands;
using PrintPlatform.Application.Identity.DTOs;
using PrintPlatform.Application.Identity.Queries;

namespace PrintPlatform.API.Controllers;

[Authorize]
[Route("api/profile")]
[Produces("application/json")]
public sealed class ProfileController : ApiControllerBase
{
    /// <summary>Returns the authenticated user's profile.</summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetCurrentUserQuery(CurrentUserId), ct);
        return ToActionResult(result);
    }

    /// <summary>Completes printer-owner onboarding for the authenticated user.</summary>
    [HttpPut("printer-owner/onboarding")]
    [ProducesResponseType(typeof(PrinterOwnerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompletePrinterOwnerOnboarding(
        [FromBody] CompletePrinterOwnerOnboardingRequest request,
        CancellationToken ct)
    {
        var result = await Mediator.Send(
            new CompletePrinterOwnerOnboardingCommand(
                CurrentUserId,
                request.BankAccountDetails,
                request.InstapayNumber),
            ct);

        return ToActionResult(result);
    }
}

public sealed record CompletePrinterOwnerOnboardingRequest(
    string BankAccountDetails,
    string? InstapayNumber);
